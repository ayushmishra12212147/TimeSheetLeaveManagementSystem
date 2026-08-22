using ReportService.Clients;
using ReportService.DTOs;

namespace ReportService.Services
{
    public class AttendanceReportService : IAttendanceReportService
    {
        private readonly IAttendanceClient _attendanceClient;
        private readonly IHolidayClient _holidayClient;
        private readonly ILeaveClient _leaveClient;
        private readonly IReportScopeResolver _reportScopeResolver;

        public AttendanceReportService(
            IAttendanceClient attendanceClient,
            IHolidayClient holidayClient,
            ILeaveClient leaveClient,
            IReportScopeResolver reportScopeResolver)
        {
            _attendanceClient = attendanceClient;
            _holidayClient = holidayClient;
            _leaveClient = leaveClient;
            _reportScopeResolver = reportScopeResolver;
        }

        public async Task<AttendanceReportResponseDto> GenerateAsync(AttendanceReportRequestDto request, CancellationToken cancellationToken = default)
        {
            var (dateFrom, dateTo) = NormalizeRange(request.DateFrom, request.DateTo);
            var employees = await _reportScopeResolver.ResolveEmployeesAsync(request.EmployeeId, cancellationToken);
            var attendanceRecords = await _attendanceClient.GetAttendanceRecordsAsync(dateFrom, dateTo, request.EmployeeId, cancellationToken);
            var holidays = await GetHolidaysAsync(dateFrom, dateTo, cancellationToken);
            var holidaysByDate = holidays.ToDictionary(x => x.HolidayDate);

            var approvedLeaveDays = new Dictionary<(string EmployeeId, DateOnly Date), string>();
            foreach (var employee in employees)
            {
                var leaves = await _leaveClient.GetLeaveRequestsAsync(employee.EmployeeId, cancellationToken);
                foreach (var leave in leaves.Where(x => string.Equals(x.Status, "Approved", StringComparison.OrdinalIgnoreCase)))
                {
                    foreach (var date in EachDate(leave.StartDate, leave.EndDate))
                    {
                        if (date < dateFrom || date > dateTo || date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            continue;
                        }

                        approvedLeaveDays[(employee.EmployeeId, date)] = leave.LeaveTypeName;
                    }
                }
            }

            var attendanceByEmployeeAndDate = attendanceRecords
                .GroupBy(x => (x.EmployeeId, x.AttendanceDate))
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.ClockOutAtUtc ?? y.ClockInAtUtc ?? DateTime.MinValue).First());

            var rows = new List<AttendanceReportRowDto>();
            foreach (var employee in employees.OrderBy(x => x.FullName))
            {
                foreach (var date in EachDate(dateFrom, dateTo))
                {
                    if (date.DayOfWeek == DayOfWeek.Sunday)
                    {
                        continue;
                    }

                    attendanceByEmployeeAndDate.TryGetValue((employee.EmployeeId, date), out var record);
                    holidaysByDate.TryGetValue(date, out var holiday);
                    approvedLeaveDays.TryGetValue((employee.EmployeeId, date), out var leaveTypeName);

                    if (record != null)
                    {
                        rows.Add(new AttendanceReportRowDto
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.FullName,
                            Date = date,
                            Status = record.Status,
                            ClockInAtUtc = record.ClockInAtUtc,
                            ClockOutAtUtc = record.ClockOutAtUtc,
                            DurationMinutes = record.DurationMinutes,
                            ScannedInByManagerName = record.ScannedInByManagerName,
                            ScannedOutByManagerName = record.ScannedOutByManagerName,
                            IsHoliday = holiday != null,
                            HolidayName = holiday?.Name,
                            IsOnApprovedLeave = leaveTypeName != null,
                            LeaveTypeName = leaveTypeName
                        });
                        continue;
                    }

                    if (holiday != null)
                    {
                        rows.Add(new AttendanceReportRowDto
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.FullName,
                            Date = date,
                            Status = "Holiday",
                            IsHoliday = true,
                            HolidayName = holiday.Name
                        });
                        continue;
                    }

                    if (leaveTypeName != null)
                    {
                        rows.Add(new AttendanceReportRowDto
                        {
                            EmployeeId = employee.EmployeeId,
                            EmployeeName = employee.FullName,
                            Date = date,
                            Status = "OnLeave",
                            IsOnApprovedLeave = true,
                            LeaveTypeName = leaveTypeName
                        });
                        continue;
                    }

                    rows.Add(new AttendanceReportRowDto
                    {
                        EmployeeId = employee.EmployeeId,
                        EmployeeName = employee.FullName,
                        Date = date,
                        Status = "Absent"
                    });
                }
            }

            var workdayRows = rows.Where(x => !x.IsHoliday).ToList();
            var durationRows = workdayRows.Where(x => x.DurationMinutes.HasValue && x.DurationMinutes.Value > 0).ToList();

            return new AttendanceReportResponseDto
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                Scope = await _reportScopeResolver.DescribeScopeAsync(request.EmployeeId, cancellationToken),
                Rows = rows,
                Summary = new AttendanceReportSummaryDto
                {
                    TotalWorkdays = workdayRows.Count,
                    PresentCount = workdayRows.Count(x => string.Equals(x.Status, "Present", StringComparison.OrdinalIgnoreCase)),
                    HalfDayCount = workdayRows.Count(x => string.Equals(x.Status, "HalfDay", StringComparison.OrdinalIgnoreCase)),
                    PendingClockOutCount = workdayRows.Count(x => string.Equals(x.Status, "PendingClockOut", StringComparison.OrdinalIgnoreCase)),
                    AbsentCount = workdayRows.Count(x => string.Equals(x.Status, "Absent", StringComparison.OrdinalIgnoreCase)),
                    OnLeaveCount = workdayRows.Count(x => string.Equals(x.Status, "OnLeave", StringComparison.OrdinalIgnoreCase)),
                    HolidayCount = rows.Count(x => x.IsHoliday),
                    AverageDurationHours = durationRows.Count == 0
                        ? 0m
                        : Math.Round((decimal)durationRows.Average(x => x.DurationMinutes!.Value) / 60m, 2)
                }
            };
        }

        private async Task<List<DownstreamHolidayResponseDto>> GetHolidaysAsync(DateOnly dateFrom, DateOnly dateTo, CancellationToken cancellationToken)
        {
            var years = Enumerable.Range(dateFrom.Year, dateTo.Year - dateFrom.Year + 1).Distinct().ToList();
            var holidays = new List<DownstreamHolidayResponseDto>();

            foreach (var year in years)
            {
                holidays.AddRange(await _holidayClient.GetHolidaysAsync(year, cancellationToken));
            }

            return holidays
                .Where(x => x.HolidayDate >= dateFrom && x.HolidayDate <= dateTo)
                .DistinctBy(x => x.HolidayDate)
                .ToList();
        }

        private static (DateOnly DateFrom, DateOnly DateTo) NormalizeRange(DateOnly? dateFrom, DateOnly? dateTo)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = dateFrom ?? new DateOnly(today.Year, today.Month, 1);
            var to = dateTo ?? today;
            return from <= to ? (from, to) : (to, from);
        }

        private static IEnumerable<DateOnly> EachDate(DateOnly start, DateOnly end)
        {
            for (var date = start; date <= end; date = date.AddDays(1))
            {
                yield return date;
            }
        }
    }
}
