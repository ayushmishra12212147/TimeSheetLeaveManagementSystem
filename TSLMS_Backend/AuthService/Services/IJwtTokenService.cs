using AuthService.Models;

namespace AuthService.Services
{
    public interface IJwtTokenService
    {
        DateTime GetAccessTokenExpiryUtc();
        string GenerateAccessToken(EmployeeUser user, DateTime expiresAtUtc);
    }
}
