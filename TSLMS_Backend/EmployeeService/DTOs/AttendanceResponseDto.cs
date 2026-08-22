namespace EmployeeService.DTOs
{
    public class AttendanceResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public DateTime? ClockInAtUtc { get; set; }
        public DateTime? ClockOutAtUtc { get; set; }
        public int? DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? ScannedInByManagerId { get; set; }
        public string? ScannedInByManagerName { get; set; }
        public Guid? ScannedOutByManagerId { get; set; }
        public string? ScannedOutByManagerName { get; set; }
    }
}
