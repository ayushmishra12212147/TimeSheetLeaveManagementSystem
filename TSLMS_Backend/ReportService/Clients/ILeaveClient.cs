using ReportService.DTOs;

namespace ReportService.Clients
{
    public interface ILeaveClient
    {
        Task<IReadOnlyCollection<DownstreamLeaveRequestResponseDto>> GetLeaveRequestsAsync(
            string? employeeId = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<DownstreamLeaveBalanceResponseDto>> GetLeaveBalancesAsync(
            string employeeId,
            int? year = null,
            CancellationToken cancellationToken = default);
    }
}
