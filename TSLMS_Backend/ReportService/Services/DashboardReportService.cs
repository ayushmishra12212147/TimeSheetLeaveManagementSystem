using ReportService.DTOs;

namespace ReportService.Services
{
    public class DashboardReportService : IDashboardReportService
    {
        private readonly ILeaveReportService _leaveReportService;
        private readonly ITimesheetReportService _timesheetReportService;
        private readonly IAttendanceReportService _attendanceReportService;
        private readonly IReportScopeResolver _reportScopeResolver;

        public DashboardReportService(
            ILeaveReportService leaveReportService,
            ITimesheetReportService timesheetReportService,
            IAttendanceReportService attendanceReportService,
            IReportScopeResolver reportScopeResolver)
        {
            _leaveReportService = leaveReportService;
            _timesheetReportService = timesheetReportService;
            _attendanceReportService = attendanceReportService;
            _reportScopeResolver = reportScopeResolver;
        }

        public async Task<DashboardReportResponseDto> GenerateAsync(DateOnly? dateFrom, DateOnly? dateTo, string? employeeId, CancellationToken cancellationToken = default)
        {
            var leaveReport = await _leaveReportService.GenerateAsync(new LeaveReportRequestDto
            {
                EmployeeId = employeeId,
                DateFrom = dateFrom,
                DateTo = dateTo
            }, cancellationToken);

            var timesheetReport = await _timesheetReportService.GenerateAsync(new TimesheetReportRequestDto
            {
                EmployeeId = employeeId,
                DateFrom = dateFrom,
                DateTo = dateTo
            }, cancellationToken);
            var attendanceReport = await _attendanceReportService.GenerateAsync(new AttendanceReportRequestDto
            {
                EmployeeId = employeeId,
                DateFrom = dateFrom,
                DateTo = dateTo
            }, cancellationToken);

            var employees = await _reportScopeResolver.ResolveEmployeesAsync(employeeId, cancellationToken);

            return new DashboardReportResponseDto
            {
                DateFrom = leaveReport.DateFrom,
                DateTo = leaveReport.DateTo,
                Scope = leaveReport.Scope,
                EmployeeCount = employees.Count,
                LeaveRequestCount = leaveReport.Summary.TotalRequests,
                LeaveRequestedDays = leaveReport.Summary.TotalRequestedDays,
                LeaveApprovedDays = leaveReport.Summary.ApprovedDays,
                LeavePendingDays = leaveReport.Summary.PendingDays,
                PendingLeaveApprovals = leaveReport.Rows.Count(x => !string.IsNullOrWhiteSpace(x.PendingApprovalRole)),
                TimesheetCount = timesheetReport.Summary.TotalWeeks,
                TimesheetHours = timesheetReport.Summary.TotalHours,
                PendingTimesheetApprovals = timesheetReport.Summary.SubmittedCount,
                LateTimesheetSubmissions = timesheetReport.Summary.LateSubmissionCount,
                RejectedTimesheets = timesheetReport.Summary.RejectedCount,
                AverageTimesheetHoursPerWeek = timesheetReport.Summary.AverageHoursPerWeek,
                AttendancePresentCount = attendanceReport.Summary.PresentCount,
                AttendanceHalfDayCount = attendanceReport.Summary.HalfDayCount,
                AttendancePendingClockOutCount = attendanceReport.Summary.PendingClockOutCount,
                AttendanceAbsentCount = attendanceReport.Summary.AbsentCount,
                AttendanceOnLeaveCount = attendanceReport.Summary.OnLeaveCount,
                AttendanceHolidayCount = attendanceReport.Summary.HolidayCount,
                AverageAttendanceHours = attendanceReport.Summary.AverageDurationHours
            };
        }
    }
}
