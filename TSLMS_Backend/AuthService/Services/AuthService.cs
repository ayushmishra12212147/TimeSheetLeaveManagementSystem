using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.DTOs;
using AuthService.Events;
using AuthService.Exceptions;
using AuthService.Messaging;
using AuthService.Models;
using AuthService.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AuthService.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _authDbContext;
        private readonly EmployeeIdentityDbContext _employeeDbContext;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRabbitMQPublisher _publisher;
        private readonly JwtOptions _jwtOptions;

        public AuthService(
            AuthDbContext authDbContext,
            EmployeeIdentityDbContext employeeDbContext,
            IJwtTokenService jwtTokenService,
            IRabbitMQPublisher publisher,
            IOptions<JwtOptions> jwtOptions)
        {
            _authDbContext = authDbContext;
            _employeeDbContext = employeeDbContext;
            _jwtTokenService = jwtTokenService;
            _publisher = publisher;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<(LoginResponseDto Response, string RefreshToken)> LoginAsync(
            LoginRequestDto dto,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            var employeeId = NormalizeEmployeeId(dto.EmployeeId);
            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            if (user == null)
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid employee ID or password.");
            }

            var lockoutEndUtc = await GetLockoutEndUtcAsync(user.Id, cancellationToken);
            if (lockoutEndUtc.HasValue && lockoutEndUtc.Value > DateTime.UtcNow)
            {
                throw new ApiException(StatusCodes.Status423Locked, "Account is locked for 15 minutes due to multiple failed login attempts.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
            {
                await RecordLoginAttemptAsync(user, false, ipAddress, cancellationToken);

                var updatedLockoutEndUtc = await GetLockoutEndUtcAsync(user.Id, cancellationToken);
                if (updatedLockoutEndUtc.HasValue && updatedLockoutEndUtc.Value > DateTime.UtcNow)
                {
                    throw new ApiException(StatusCodes.Status423Locked, "Account is locked for 15 minutes due to multiple failed login attempts.");
                }

                throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid employee ID or password.");
            }

            await ClearFailedAttemptsAsync(user.Id, cancellationToken);
            await RecordLoginAttemptAsync(user, true, ipAddress, cancellationToken);

            return await CreateSessionAsync(user, ipAddress, cancellationToken);
        }

        public async Task<(LoginResponseDto Response, string RefreshToken)> RefreshAsync(
            string refreshToken,
            string? ipAddress,
            CancellationToken cancellationToken = default)
        {
            var tokenHash = ComputeHash(refreshToken);
            var storedToken = await _authDbContext.RefreshTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow,
                    cancellationToken);

            if (storedToken == null)
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Refresh token is invalid or expired.");
            }

            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == storedToken.UserId, cancellationToken);

            if (user == null)
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "User account was not found.");
            }

            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await _authDbContext.SaveChangesAsync(cancellationToken);

            return await CreateSessionAsync(user, ipAddress, cancellationToken);
        }

        public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            await RevokeRefreshTokensAsync(userId, cancellationToken);
            await _authDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
        {
            var employeeId = NormalizeEmployeeId(dto.EmployeeId);
            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId, cancellationToken);

            if (user == null)
            {
                return false;
            }

            var activeTokens = await _authDbContext.PasswordResetTokens
                .Where(x => x.UserId == user.Id && x.UsedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.UsedAtUtc = DateTime.UtcNow;
            }

            var rawToken = GenerateToken();
            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = ComputeHash(rawToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(30)
            };

            _authDbContext.PasswordResetTokens.Add(resetToken);
            await _authDbContext.SaveChangesAsync(cancellationToken);

            var resetRequestedEvent = new PasswordResetRequestedEvent
            {
                UserId = user.Id,
                EmployeeId = user.EmployeeId,
                Email = user.Email,
                FullName = user.FullName,
                ResetToken = rawToken,
                ExpiresAtUtc = resetToken.ExpiresAtUtc
            };

            _publisher.Publish(resetRequestedEvent, "password.reset.requested");
            return true;
        }

        public async Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
        {
            var tokenHash = ComputeHash(dto.Token);
            var resetToken = await _authDbContext.PasswordResetTokens
                .FirstOrDefaultAsync(
                    x => x.TokenHash == tokenHash && x.UsedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow,
                    cancellationToken);

            if (resetToken == null)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Reset token is invalid or expired.");
            }

            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == resetToken.UserId, cancellationToken);

            if (user == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "User account was not found.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsFirstLogin = false;
            user.MustResetPassword = false;
            user.TempPasswordExpiry = null;

            resetToken.UsedAtUtc = DateTime.UtcNow;

            await RevokeRefreshTokensAsync(user.Id, cancellationToken);
            await ClearFailedAttemptsAsync(user.Id, cancellationToken);
            await _employeeDbContext.SaveChangesAsync(cancellationToken);
            await _authDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task FirstLoginResetPasswordAsync(
            Guid userId,
            FirstLoginResetPasswordDto dto,
            CancellationToken cancellationToken = default)
        {
            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "User account was not found.");
            }

            if (!user.IsFirstLogin && !user.MustResetPassword)
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "First-login password reset is not required.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                throw new ApiException(StatusCodes.Status400BadRequest, "Current password is incorrect.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsFirstLogin = false;
            user.MustResetPassword = false;
            user.TempPasswordExpiry = null;

            await RevokeRefreshTokensAsync(user.Id, cancellationToken);
            await ClearFailedAttemptsAsync(user.Id, cancellationToken);
            await _employeeDbContext.SaveChangesAsync(cancellationToken);
            await _authDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<UserSummaryDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _employeeDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user == null)
            {
                throw new ApiException(StatusCodes.Status404NotFound, "User account was not found.");
            }

            return MapUser(user);
        }

        private async Task<(LoginResponseDto Response, string RefreshToken)> CreateSessionAsync(
            EmployeeUser user,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var expiresAtUtc = _jwtTokenService.GetAccessTokenExpiryUtc();
            var accessToken = _jwtTokenService.GenerateAccessToken(user, expiresAtUtc);
            var refreshToken = GenerateToken();

            _authDbContext.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = ComputeHash(refreshToken),
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                CreatedByIp = ipAddress
            });

            await _authDbContext.SaveChangesAsync(cancellationToken);

            return (
                new LoginResponseDto
                {
                    AccessToken = accessToken,
                    ExpiresAtUtc = expiresAtUtc,
                    User = MapUser(user)
                },
                refreshToken);
        }

        private async Task<DateTime?> GetLockoutEndUtcAsync(Guid userId, CancellationToken cancellationToken)
        {
            var failureWindowStart = DateTime.UtcNow.AddMinutes(-10);

            var recentFailures = await _authDbContext.LoginAttempts
                .Where(x => x.UserId == userId && !x.WasSuccessful && x.AttemptedAtUtc >= failureWindowStart)
                .OrderByDescending(x => x.AttemptedAtUtc)
                .ToListAsync(cancellationToken);

            if (recentFailures.Count < 5)
            {
                return null;
            }

            var lastFailure = recentFailures.Max(x => x.AttemptedAtUtc);
            var lockoutEndUtc = lastFailure.AddMinutes(15);

            return lockoutEndUtc > DateTime.UtcNow
                ? lockoutEndUtc
                : null;
        }

        private async Task RecordLoginAttemptAsync(
            EmployeeUser user,
            bool wasSuccessful,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            _authDbContext.LoginAttempts.Add(new LoginAttempt
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EmployeeId = user.EmployeeId,
                AttemptedAtUtc = DateTime.UtcNow,
                WasSuccessful = wasSuccessful,
                IpAddress = ipAddress
            });

            await _authDbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task ClearFailedAttemptsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var failedAttempts = await _authDbContext.LoginAttempts
                .Where(x => x.UserId == userId && !x.WasSuccessful)
                .ToListAsync(cancellationToken);

            if (failedAttempts.Count == 0)
            {
                return;
            }

            _authDbContext.LoginAttempts.RemoveRange(failedAttempts);
            await _authDbContext.SaveChangesAsync(cancellationToken);
        }

        private async Task RevokeRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
        {
            var activeTokens = await _authDbContext.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAtUtc = DateTime.UtcNow;
            }
        }

        private static string NormalizeEmployeeId(string employeeId)
        {
            return employeeId.Trim().ToUpperInvariant();
        }

        private static string GenerateToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private static string ComputeHash(string rawValue)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawValue));
            return Convert.ToHexString(bytes);
        }

        private static UserSummaryDto MapUser(EmployeeUser user)
        {
            return new UserSummaryDto
            {
                Id = user.Id,
                EmployeeId = user.EmployeeId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                Gender = user.Gender,
                DepartmentId = user.DepartmentId,
                ManagerId = user.ManagerId,
                IsFirstLogin = user.IsFirstLogin,
                MustResetPassword = user.MustResetPassword,
                IsProfileComplete = user.IsProfileComplete
            };
        }
    }
}
