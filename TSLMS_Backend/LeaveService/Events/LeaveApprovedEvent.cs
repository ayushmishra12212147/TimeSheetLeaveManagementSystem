namespace LeaveService.Events
{
    public class LeaveApprovedEvent
    {
        public Guid RecipientUserId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public Guid LeaveRequestId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }
}
