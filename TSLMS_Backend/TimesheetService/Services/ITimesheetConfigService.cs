using TimesheetService.DTOs;

namespace TimesheetService.Services
{
    public interface ITimesheetConfigService
    {
        Task<TimesheetConfigResponseDto> GetAsync(CancellationToken cancellationToken = default);
        Task<TimesheetConfigResponseDto> UpdateAsync(UpdateTimesheetConfigDto dto, CancellationToken cancellationToken = default);
    }
}
