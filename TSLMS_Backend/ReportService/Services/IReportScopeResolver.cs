using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IReportScopeResolver
    {
        Task<IReadOnlyCollection<EmployeeDirectoryUserDto>> ResolveEmployeesAsync(string? employeeId, CancellationToken cancellationToken = default);
        Task<string> DescribeScopeAsync(string? employeeId, CancellationToken cancellationToken = default);
    }
}
