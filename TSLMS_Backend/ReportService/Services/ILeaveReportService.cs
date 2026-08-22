using ReportService.DTOs;

namespace ReportService.Services
{
    public interface ILeaveReportService
    {
        Task<LeaveReportResponseDto> GenerateAsync(LeaveReportRequestDto request, CancellationToken cancellationToken = default);
    }
}
