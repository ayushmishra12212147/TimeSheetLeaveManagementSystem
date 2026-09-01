namespace LeaveService.DTOs
{
    public class CreateLeaveTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal DefaultAnnualQuota { get; set; }
        public decimal MaxCarryForwardDays { get; set; }
        public bool RequiresDocument { get; set; }
        public bool IsAutoApprove { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
