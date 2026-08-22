using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Helpers;
using AuthService.Options;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IAuthService authService, IOptions<JwtOptions> jwtOptions)
        {
            _authService = authService;
            _jwtOptions = jwtOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
        {
            var (response, refreshToken) = await _authService.LoginAsync(dto, GetIpAddress(), cancellationToken);
            AppendRefreshCookie(refreshToken);

            return Ok(new ApiResponse<LoginResponseDto>(response, "Login successful."));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            if (!Request.Cookies.TryGetValue(_jwtOptions.RefreshCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(new ApiResponse<object>(null, "Refresh token cookie is missing.", false));
            }

            var (response, newRefreshToken) = await _authService.RefreshAsync(refreshToken, GetIpAddress(), cancellationToken);
            AppendRefreshCookie(newRefreshToken);

            return Ok(new ApiResponse<LoginResponseDto>(response, "Token refreshed successfully."));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            await _authService.LogoutAsync(GetCurrentUserId(), cancellationToken);
            DeleteRefreshCookie();

            return Ok(new ApiResponse<object>(null, "Logout successful."));
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken)
        {
            var wasQueued = await _authService.ForgotPasswordAsync(dto, cancellationToken);

            if (!wasQueued)
            {
                return NotFound(new ApiResponse<object>(null, "Employee ID not found.", false));
            }

            return Ok(new ApiResponse<object>(null, "Password reset link has been sent to the registered email address."));
        }

        [AllowAnonymous]
        [HttpGet("reset-password")]
        public IActionResult ResetPasswordInfo([FromQuery] string? token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new ApiResponse<object>(null, "Reset token is missing from the URL.", false));
            }

            return Ok(new ApiResponse<object>(
                new
                {
                    token,
                    method = "POST",
                    endpoint = "/api/v1/auth/reset-password",
                    sampleBody = new
                    {
                        token,
                        newPassword = "NewPass123!",
                        confirmPassword = "NewPass123!"
                    }
                },
                "Use this token in the POST reset-password API to complete the password reset."));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
        {
            await _authService.ResetPasswordAsync(dto, cancellationToken);
            DeleteRefreshCookie();

            return Ok(new ApiResponse<object>(null, "Password has been reset successfully."));
        }

        [Authorize]
        [HttpPost("first-login/reset-password")]
        public async Task<IActionResult> FirstLoginResetPassword([FromBody] FirstLoginResetPasswordDto dto, CancellationToken cancellationToken)
        {
            await _authService.FirstLoginResetPasswordAsync(GetCurrentUserId(), dto, cancellationToken);
            DeleteRefreshCookie();

            return Ok(new ApiResponse<object>(null, "First-login password reset completed successfully."));
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken cancellationToken)
        {
            var response = await _authService.GetMeAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new ApiResponse<UserSummaryDto>(response, "User profile fetched successfully."));
        }

        private Guid GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(subject!);
        }

        private string? GetIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private void AppendRefreshCookie(string refreshToken)
        {
            Response.Cookies.Append(_jwtOptions.RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays)
            });
        }

        private void DeleteRefreshCookie()
        {
            Response.Cookies.Delete(_jwtOptions.RefreshCookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
