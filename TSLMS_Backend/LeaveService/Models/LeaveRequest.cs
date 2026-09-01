using LeaveService.Enums;

namespace LeaveService.Models
{
    public class LeaveRequest
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public Guid? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmail { get; set; }
        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public bool IsHalfDay { get; set; }
        public HalfDaySession HalfDaySession { get; set; }
        public bool IsUnpaid { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SupportingDocumentUrl { get; set; }
        public LeaveStatus Status { get; set; }
        public ApprovalRole? PendingApprovalRole { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByName { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
