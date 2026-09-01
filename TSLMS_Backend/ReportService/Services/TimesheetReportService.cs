using ReportService.Clients;
using ReportService.DTOs;

namespace ReportService.Services
{
    public class TimesheetReportService : ITimesheetReportService
    {
        private readonly ITimesheetClient _timesheetClient;
        private readonly IReportScopeResolver _reportScopeResolver;

        public TimesheetReportService(ITimesheetClient timesheetClient, IReportScopeResolver reportScopeResolver)
        {
            _timesheetClient = timesheetClient;
            _reportScopeResolver = reportScopeResolver;
        }

        public async Task<TimesheetReportResponseDto> GenerateAsync(TimesheetReportRequestDto request, CancellationToken cancellationToken = default)
        {
            var (dateFrom, dateTo) = NormalizeRange(request.DateFrom, request.DateTo);
            await _reportScopeResolver.ResolveEmployeesAsync(request.EmployeeId, cancellationToken);

            var summaries = new List<TimesheetReportRowDto>();
            foreach (var weekStart in GetWeekStarts(dateFrom, dateTo))
            {
                var weekSummaries = await _timesheetClient.GetTeamTimesheetsAsync(weekStart, request.EmployeeId, cancellationToken);
                foreach (var item in weekSummaries.Where(x => OverlapsRange(x.WeekStartDate, x.WeekEndDate, dateFrom, dateTo)))
                {
                    if (!string.IsNullOrWhiteSpace(request.Status) &&
                        !string.Equals(item.Status, request.Status, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    summaries.Add(new TimesheetReportRowDto
                    {
                        SummaryId = item.Id,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmployeeName,
                        ManagerName = item.ManagerName,
                        WeekStartDate = item.WeekStartDate,
                        WeekEndDate = item.WeekEndDate,
                        TotalHours = item.TotalHours,
                        EntryCount = item.EntryCount,
                        Status = item.Status,
                        IsLateSubmission = item.IsLateSubmission,
                        MeetsMinimumWeeklyHours = item.MeetsMinimumWeeklyHours,
                        SubmittedAtUtc = item.SubmittedAtUtc,
                        ApprovedByName = item.ApprovedByName,
                        ApprovedAtUtc = item.ApprovedAtUtc,
                        RejectedByName = item.RejectedByName,
                        RejectionReason = item.RejectionReason,
                        RejectedAtUtc = item.RejectedAtUtc
                    });
                }
            }

            summaries = summaries
                .DistinctBy(x => x.SummaryId)
                .OrderByDescending(x => x.WeekStartDate)
                .ThenBy(x => x.EmployeeName)
                .ToList();

            return new TimesheetReportResponseDto
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Scope = await _reportScopeResolver.DescribeScopeAsync(request.EmployeeId, cancellationToken),
                Rows = summaries,
                Summary = new TimesheetReportSummaryDto
                {
                    TotalWeeks = summaries.Count,
                    TotalHours = summaries.Sum(x => x.TotalHours),
                    AverageHoursPerWeek = summaries.Count == 0 ? 0m : Math.Round(summaries.Average(x => x.TotalHours), 2),
                    ApprovedCount = summaries.Count(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase)),
                    SubmittedCount = summaries.Count(x => string.Equals(x.Status, "Submitted", StringComparison.OrdinalIgnoreCase)),
                    RejectedCount = summaries.Count(x => string.Equals(x.Status, "Rejected", StringComparison.OrdinalIgnoreCase)),
                    DraftCount = summaries.Count(x => string.Equals(x.Status, "Draft", StringComparison.OrdinalIgnoreCase)),
                    LateSubmissionCount = summaries.Count(x => x.IsLateSubmission),
                    MinimumHoursMetCount = summaries.Count(x => x.MeetsMinimumWeeklyHours)
                }
            };
        }

        private static (DateOnly DateFrom, DateOnly DateTo) NormalizeRange(DateOnly? dateFrom, DateOnly? dateTo)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = dateFrom ?? new DateOnly(today.Year, today.Month, 1);
            var to = dateTo ?? today;
            return from <= to ? (from, to) : (to, from);
        }

        private static IEnumerable<DateOnly> GetWeekStarts(DateOnly dateFrom, DateOnly dateTo)
        {
            var current = AlignToMonday(dateFrom);
            var last = AlignToMonday(dateTo);

            while (current <= last)
            {
                yield return current;
                current = current.AddDays(7);
            }
        }

        private static DateOnly AlignToMonday(DateOnly date)
        {
            if (date.DayOfWeek == DayOfWeek.Sunday)
            {
                return date.AddDays(-6);
            }

            var delta = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            return date.AddDays(-delta);
        }

        private static bool OverlapsRange(DateOnly start, DateOnly end, DateOnly from, DateOnly to)
        {
            return start <= to && end >= from;
        }
    }
}
