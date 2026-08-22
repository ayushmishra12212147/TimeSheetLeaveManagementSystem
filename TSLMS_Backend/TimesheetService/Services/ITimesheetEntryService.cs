using TimesheetService.DTOs;

namespace TimesheetService.Services
{
    public interface ITimesheetEntryService
    {
        Task<WeekTimesheetResponseDto> GetWeekAsync(DateOnly? weekStartDate, string? employeeId, CancellationToken cancellationToken = default);
        Task<TimesheetEntryResponseDto> CreateAsync(CreateTimesheetEntryDto dto, CancellationToken cancellationToken = default);
        Task<TimesheetEntryResponseDto> UpdateAsync(Guid id, UpdateTimesheetEntryDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<WeeklyTimesheetSummaryResponseDto> SubmitAsync(SubmitTimesheetDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>> GetPendingAsync(DateOnly? weekStartDate, CancellationToken cancellationToken = default);
        Task<WeeklyTimesheetSummaryResponseDto> ApproveAsync(Guid summaryId, ApproveTimesheetDto dto, CancellationToken cancellationToken = default);
        Task<WeeklyTimesheetSummaryResponseDto> RejectAsync(Guid summaryId, RejectTimesheetDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyCollection<WeeklyTimesheetSummaryResponseDto>> GetTeamAsync(DateOnly? weekStartDate, string? employeeId, CancellationToken cancellationToken = default);
        Task<int> AutoApproveExpiredSubmittedAsync(CancellationToken cancellationToken = default);
    }
}
