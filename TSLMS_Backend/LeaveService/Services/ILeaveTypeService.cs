using LeaveService.DTOs;

namespace LeaveService.Services
{
    public interface ILeaveTypeService
    {
        Task<IReadOnlyCollection<LeaveTypeResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<LeaveTypeResponseDto> CreateAsync(CreateLeaveTypeDto dto, CancellationToken cancellationToken = default);
        Task<LeaveTypeResponseDto> UpdateAsync(Guid id, UpdateLeaveTypeDto dto, CancellationToken cancellationToken = default);
        Task<LeaveTypeResponseDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
