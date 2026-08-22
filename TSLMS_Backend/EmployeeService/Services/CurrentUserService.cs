using System.Security.Claims;
using EmployeeService.Exceptions;

namespace EmployeeService.Services
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
            var subject = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("sub");

            if (!Guid.TryParse(subject, out var userId))
            {
                throw new ApiException(StatusCodes.Status401Unauthorized, "Authenticated user context is missing.");
            }

            return userId;
        }

        public string GetEmployeeId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue("employee_id")
                ?? throw new ApiException(StatusCodes.Status401Unauthorized, "Employee ID claim is missing.");
        }

        public string GetRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
                ?? string.Empty;
        }
    }
}
