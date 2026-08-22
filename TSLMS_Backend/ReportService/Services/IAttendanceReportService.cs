using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IAttendanceReportService
    {
        Task<AttendanceReportResponseDto> GenerateAsync(AttendanceReportRequestDto request, CancellationToken cancellationToken = default);
    }
}
