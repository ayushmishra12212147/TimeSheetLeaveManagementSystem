namespace LeaveService.Events
{
    public class ManagerAssignmentChangedEvent
    {
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public Guid? PreviousManagerUserId { get; set; }
        public string? PreviousManagerName { get; set; }
        public string? PreviousManagerEmail { get; set; }
        public Guid? CurrentManagerUserId { get; set; }
        public string? CurrentManagerName { get; set; }
        public string? CurrentManagerEmail { get; set; }
        public string Action { get; set; } = string.Empty;
        public DateTime ChangedAtUtc { get; set; }
    }
}
