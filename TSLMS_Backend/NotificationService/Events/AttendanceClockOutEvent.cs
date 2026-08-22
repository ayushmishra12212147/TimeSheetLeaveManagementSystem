namespace NotificationService.Events
{
    public class AttendanceClockOutEvent
    {
        public Guid RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public Guid AttendanceRecordId { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public DateTime ClockOutAtUtc { get; set; }
        public int DurationMinutes { get; set; }
        public Guid ScannedByManagerId { get; set; }
        public string ScannedByManagerName { get; set; } = string.Empty;
    }
}
