using ReportService.Clients;
using ReportService.DTOs;
using ReportService.Exceptions;

namespace ReportService.Services
{
    public class ReportScopeResolver : IReportScopeResolver
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IEmployeeDirectoryClient _employeeDirectoryClient;

        public ReportScopeResolver(ICurrentUserService currentUserService, IEmployeeDirectoryClient employeeDirectoryClient)
        {
            _currentUserService = currentUserService;
            _employeeDirectoryClient = employeeDirectoryClient;
        }

        public async Task<IReadOnlyCollection<EmployeeDirectoryUserDto>> ResolveEmployeesAsync(string? employeeId, CancellationToken cancellationToken = default)
        {
            var role = _currentUserService.GetRole();
            var currentUserId = _currentUserService.GetUserId();

            if (string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    var employee = (await _employeeDirectoryClient.GetUsersAsync(employeeId: employeeId, cancellationToken: cancellationToken)).FirstOrDefault();
                    if (employee == null)
                    {
                        throw new ApiException(StatusCodes.Status404NotFound, "Employee not found.");
                    }

                    return [employee];
                }

                return await _employeeDirectoryClient.GetUsersAsync(cancellationToken: cancellationToken);
            }

            if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                var reports = await _employeeDirectoryClient.GetUsersAsync(
                    managerId: currentUserId,
                    employeeId: employeeId,
                    cancellationToken: cancellationToken);

                if (!string.IsNullOrWhiteSpace(employeeId) && reports.Count == 0)
                {
                    throw new ApiException(StatusCodes.Status403Forbidden, "Managers can only prepare reports for their direct reports.");
                }

                return reports;
            }

            throw new ApiException(StatusCodes.Status403Forbidden, "Only managers and HRAdmin can access reports.");
        }

        public async Task<string> DescribeScopeAsync(string? employeeId, CancellationToken cancellationToken = default)
        {
            var role = _currentUserService.GetRole();
            if (!string.IsNullOrWhiteSpace(employeeId))
            {
                var employee = (await ResolveEmployeesAsync(employeeId, cancellationToken)).FirstOrDefault();
                return employee == null ? "Employee" : $"{employee.FullName} ({employee.EmployeeId})";
            }

            return string.Equals(role, "HRAdmin", StringComparison.OrdinalIgnoreCase)
                ? "All employees"
                : "Manager direct reports";
        }
    }
}
