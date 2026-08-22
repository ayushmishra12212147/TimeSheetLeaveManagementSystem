using TimesheetService.Enums;

namespace TimesheetService.Models
{
    public class TimesheetEntry
    {
        public Guid Id { get; set; }
        public Guid WeeklyTimesheetSummaryId { get; set; }
        public WeeklyTimesheetSummary WeeklyTimesheetSummary { get; set; } = null!;
        public DateOnly EntryDate { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string ProjectName { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public TimesheetCategory Category { get; set; }
        public string? Description { get; set; }
        public TimesheetStatus Status { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
