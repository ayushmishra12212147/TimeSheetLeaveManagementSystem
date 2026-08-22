using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IExportService
    {
        Task<ExportFileResult> ExportAttendanceAsync(AttendanceReportResponseDto report, string format, CancellationToken cancellationToken = default);
        Task<ExportFileResult> ExportLeaveAsync(LeaveReportResponseDto report, string format, CancellationToken cancellationToken = default);
        Task<ExportFileResult> ExportTimesheetAsync(TimesheetReportResponseDto report, string format, CancellationToken cancellationToken = default);
    }
}
