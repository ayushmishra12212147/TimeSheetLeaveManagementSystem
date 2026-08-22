namespace ReportService.DTOs
{
    public class LeaveReportRequestDto
    {
        public string? EmployeeId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? Status { get; set; }
        public string? LeaveType { get; set; }
    }

    public class LeaveReportRowDto
    {
        public Guid LeaveRequestId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal RequestedDays { get; set; }
        public bool IsUnpaid { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PendingApprovalRole { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    public class LeaveBalanceSnapshotDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string LeaveTypeName { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal AllocatedDays { get; set; }
        public decimal CarriedForwardDays { get; set; }
        public decimal ManualAdjustmentDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal UsedDays { get; set; }
        public decimal AvailableDays { get; set; }
    }

    public class LeaveReportSummaryDto
    {
        public int TotalRequests { get; set; }
        public decimal TotalRequestedDays { get; set; }
        public decimal ApprovedDays { get; set; }
        public decimal PendingDays { get; set; }
        public decimal RejectedDays { get; set; }
        public decimal CancelledDays { get; set; }
        public decimal UnpaidDays { get; set; }
        public Dictionary<string, int> StatusBreakdown { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class LeaveReportResponseDto
    {
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Scope { get; set; } = string.Empty;
        public IReadOnlyCollection<LeaveReportRowDto> Rows { get; set; } = Array.Empty<LeaveReportRowDto>();
        public IReadOnlyCollection<LeaveBalanceSnapshotDto> Balances { get; set; } = Array.Empty<LeaveBalanceSnapshotDto>();
        public LeaveReportSummaryDto Summary { get; set; } = new();
    }
}
