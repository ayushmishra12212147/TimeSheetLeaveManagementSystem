namespace LeaveService.Models
{
    public class LeaveBalance
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal ManualAdjustmentDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal UsedDays { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
