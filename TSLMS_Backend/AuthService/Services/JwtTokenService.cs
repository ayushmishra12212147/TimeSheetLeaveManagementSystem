using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Models;
using AuthService.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _options;

        public JwtTokenService(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public DateTime GetAccessTokenExpiryUtc()
        {
            return DateTime.UtcNow.AddHours(_options.AccessTokenExpiryHours);
        }

        public string GenerateAccessToken(EmployeeUser user, DateTime expiresAtUtc)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("employee_id", user.EmployeeId),
                new("first_login", user.IsFirstLogin.ToString().ToLowerInvariant()),
                new("gender", user.Gender ?? "Unspecified")
            };

            if (user.DepartmentId.HasValue)
            {
                claims.Add(new Claim("dept_id", user.DepartmentId.Value.ToString()));
            }

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
