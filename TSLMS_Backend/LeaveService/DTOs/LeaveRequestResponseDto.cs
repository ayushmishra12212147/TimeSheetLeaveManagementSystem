namespace LeaveService.DTOs
{
    public class LeaveRequestResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public Guid? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public bool IsHalfDay { get; set; }
        public string HalfDaySession { get; set; } = string.Empty;
        public bool IsUnpaid { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SupportingDocumentUrl { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PendingApprovalRole { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
