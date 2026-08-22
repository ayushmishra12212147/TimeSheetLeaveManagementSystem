using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LeaveService.Exceptions;

namespace LeaveService.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetUserId()
        {
            var user = GetPrincipal();
            var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(subject, out var userId))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "User identity is invalid.");
            }

            return userId;
        }

        public string GetRole()
        {
            var role = GetPrincipal().FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(role))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "User role is missing.");
            }

            return role;
        }

        public string GetEmployeeId()
        {
            var employeeId = GetPrincipal().FindFirstValue("employee_id");
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Employee ID claim is missing.");
            }

            return employeeId;
        }

        private ClaimsPrincipal GetPrincipal()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "User is not authenticated.");
            }

            return user;
        }
    }
}
