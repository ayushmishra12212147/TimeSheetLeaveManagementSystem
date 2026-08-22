using TimesheetService.DTOs;

namespace TimesheetService.Services
{
    public interface IProjectService
    {
        Task<IReadOnlyCollection<ProjectResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto, CancellationToken cancellationToken = default);
        Task<ProjectResponseDto> UpdateAsync(Guid id, UpdateProjectDto dto, CancellationToken cancellationToken = default);
        Task<ProjectResponseDto> ToggleActiveAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
