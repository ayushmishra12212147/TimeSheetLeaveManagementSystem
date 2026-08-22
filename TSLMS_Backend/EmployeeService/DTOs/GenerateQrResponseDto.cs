namespace EmployeeService.DTOs
{
    public class GenerateQrResponseDto
    {
        public string Type { get; set; } = string.Empty;
        public DateOnly AttendanceDate { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public string QrPayload { get; set; } = string.Empty;
    }
}
