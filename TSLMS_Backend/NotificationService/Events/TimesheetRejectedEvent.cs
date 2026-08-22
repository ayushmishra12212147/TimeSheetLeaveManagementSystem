namespace NotificationService.Events
{
    public class TimesheetRejectedEvent
    {
        public Guid RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public Guid TimesheetSummaryId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public string RejectedByName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
