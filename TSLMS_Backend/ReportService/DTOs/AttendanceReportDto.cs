namespace ReportService.DTOs
{
    public class AttendanceReportRequestDto
    {
        public string? EmployeeId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }

    public class AttendanceReportRowDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? ClockInAtUtc { get; set; }
        public DateTime? ClockOutAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public string? ScannedInByManagerName { get; set; }
        public string? ScannedOutByManagerName { get; set; }
        public bool IsHoliday { get; set; }
        public string? HolidayName { get; set; }
        public bool IsOnApprovedLeave { get; set; }
        public string? LeaveTypeName { get; set; }
    }

    public class AttendanceReportSummaryDto
    {
        public int TotalWorkdays { get; set; }
        public int PresentCount { get; set; }
        public int HalfDayCount { get; set; }
        public int PendingClockOutCount { get; set; }
        public int AbsentCount { get; set; }
        public int OnLeaveCount { get; set; }
        public int HolidayCount { get; set; }
        public decimal AverageDurationHours { get; set; }
    }

    public class AttendanceReportResponseDto
    {
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Scope { get; set; } = string.Empty;
        public IReadOnlyCollection<AttendanceReportRowDto> Rows { get; set; } = [];
        public AttendanceReportSummaryDto Summary { get; set; } = new();
    }
}
