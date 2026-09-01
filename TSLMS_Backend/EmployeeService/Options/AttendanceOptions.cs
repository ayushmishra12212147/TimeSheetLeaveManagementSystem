namespace EmployeeService.Options
{
    public class AttendanceOptions
    {
        public const string SectionName = "Attendance";

        public string QrSecret { get; set; } = string.Empty;
        public int QrExpiryMinutes { get; set; } = 5;
        public int HalfDayThresholdMinutes { get; set; } = 240;
    }
}
