using ReportService.DTOs;

namespace ReportService.Services
{
    public interface IReportRequestService
    {
        Task<ReportRequestResponseDto> CreateAsync(CreateReportRequestDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<ReportRequestResponseDto>> GetVisibleAsync(bool pendingOnly, CancellationToken cancellationToken = default);
        Task<ReportRequestResponseDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default);
        Task<ReportRequestResponseDto> RejectAsync(Guid id, RejectReportRequestDto dto, CancellationToken cancellationToken = default);
        Task<ExportFileResult> ExportAsync(Guid id, string format, CancellationToken cancellationToken = default);
    }
}
