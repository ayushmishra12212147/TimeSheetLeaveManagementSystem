using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<(LoginResponseDto Response, string RefreshToken)> LoginAsync(LoginRequestDto dto, string? ipAddress, CancellationToken cancellationToken = default);
        Task<(LoginResponseDto Response, string RefreshToken)> RefreshAsync(string refreshToken, string? ipAddress, CancellationToken cancellationToken = default);
        Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
        Task FirstLoginResetPasswordAsync(Guid userId, FirstLoginResetPasswordDto dto, CancellationToken cancellationToken = default);
        Task<UserSummaryDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
