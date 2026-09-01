namespace TimesheetService.Models
{
    public class TimesheetAutoApproveConfig
    {
        public Guid Id { get; set; }
        public decimal MinimumWeeklyHours { get; set; }
        public decimal LowHoursWarningThreshold { get; set; }
        public decimal HighHoursWarningThreshold { get; set; }
        public bool AutoApproveEnabled { get; set; }
        public int AutoApproveAfterHours { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
