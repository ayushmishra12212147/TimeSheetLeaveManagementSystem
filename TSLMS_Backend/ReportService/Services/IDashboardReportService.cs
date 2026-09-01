using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IDashboardReportService
    {
        Task<DashboardReportResponseDto> GenerateAsync(DateOnly? dateFrom, DateOnly? dateTo, string? employeeId, CancellationToken cancellationToken = default);
    }
}
