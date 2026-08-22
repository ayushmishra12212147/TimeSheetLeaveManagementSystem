namespace LeaveService.DTOs
{
    public class LeaveBalanceResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal ManualAdjustmentDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal AvailableDays { get; set; }
    }
}
