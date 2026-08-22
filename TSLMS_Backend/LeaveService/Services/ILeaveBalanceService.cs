using LeaveService.DTOs;

namespace LeaveService.Services
{
    public interface ILeaveBalanceService
    {
        Task<IReadOnlyCollection<LeaveBalanceResponseDto>> GetMyAsync(int? year, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<LeaveBalanceResponseDto>> GetByEmployeeAsync(string employeeId, int? year, CancellationToken cancellationToken = default);
        Task<LeaveBalanceResponseDto> AdjustAsync(Guid balanceId, AdjustBalanceDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<LeaveBalanceResponseDto>> CarryForwardAsync(CarryForwardBalanceDto dto, CancellationToken cancellationToken = default);
    }
}
