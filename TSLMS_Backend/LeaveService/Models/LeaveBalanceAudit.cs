namespace LeaveService.Models
{
    public class LeaveBalanceAudit
    {
        public Guid Id { get; set; }
        public Guid LeaveBalanceId { get; set; }
        public LeaveBalance LeaveBalance { get; set; } = null!;
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal DeltaDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid AdjustedByUserId { get; set; }
        public string AdjustedByName { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
