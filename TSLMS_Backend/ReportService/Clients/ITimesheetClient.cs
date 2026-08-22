using ReportService.DTOs;

namespace ReportService.Clients
{
    public interface ITimesheetClient
    {
        Task<IReadOnlyCollection<DownstreamWeeklyTimesheetSummaryResponseDto>> GetTeamTimesheetsAsync(
            DateOnly weekStartDate,
            string? employeeId = null,
            CancellationToken cancellationToken = default);
    }
}
