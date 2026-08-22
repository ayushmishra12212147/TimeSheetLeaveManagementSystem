using ReportService.DTOs;

namespace ReportService.Services
{
    public interface ITimesheetReportService
    {
        Task<TimesheetReportResponseDto> GenerateAsync(TimesheetReportRequestDto request, CancellationToken cancellationToken = default);
    }
}
