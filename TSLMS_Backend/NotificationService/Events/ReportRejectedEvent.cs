namespace NotificationService.Events
{
    public class ReportRejectedEvent
    {
        public Guid RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public Guid ReportRequestId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string RejectedByName { get; set; } = string.Empty;
        public string ScopeLabel { get; set; } = string.Empty;
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
