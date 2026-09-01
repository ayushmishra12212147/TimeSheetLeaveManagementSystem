using LeaveService.DTOs;

namespace LeaveService.Services
{
    public interface ILeaveRequestService
    {
        Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetVisibleAsync(string? employeeId, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<LeaveRequestResponseDto>> GetTeamCalendarAsync(DateOnly? dateFrom, DateOnly? dateTo, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> CreateAsync(CreateLeaveRequestDto dto, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> UpdateAsync(Guid id, UpdateLeaveRequestDto dto, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> ApproveAsync(Guid id, ApproveLeaveDto dto, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> RejectAsync(Guid id, RejectLeaveDto dto, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> WithdrawAsync(Guid id, CancellationToken cancellationToken = default);
        Task<LeaveRequestResponseDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
