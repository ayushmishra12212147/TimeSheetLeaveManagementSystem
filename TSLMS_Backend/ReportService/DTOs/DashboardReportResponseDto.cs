namespace ReportService.DTOs
{
    public class DashboardReportResponseDto
    {
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Scope { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public int LeaveRequestCount { get; set; }
        public decimal LeaveRequestedDays { get; set; }
        public decimal LeaveApprovedDays { get; set; }
        public decimal LeavePendingDays { get; set; }
        public int PendingLeaveApprovals { get; set; }
        public int TimesheetCount { get; set; }
        public decimal TimesheetHours { get; set; }
        public int PendingTimesheetApprovals { get; set; }
        public int LateTimesheetSubmissions { get; set; }
        public int RejectedTimesheets { get; set; }
        public decimal AverageTimesheetHoursPerWeek { get; set; }
        public int AttendancePresentCount { get; set; }
        public int AttendanceHalfDayCount { get; set; }
        public int AttendancePendingClockOutCount { get; set; }
        public int AttendanceAbsentCount { get; set; }
        public int AttendanceOnLeaveCount { get; set; }
        public int AttendanceHolidayCount { get; set; }
        public decimal AverageAttendanceHours { get; set; }
    }
}
