namespace NotificationService.Events
{
    public class TimesheetSubmittedEvent
    {
        public Guid RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public Guid TimesheetSummaryId { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public bool IsLateSubmission { get; set; }
    }
}
