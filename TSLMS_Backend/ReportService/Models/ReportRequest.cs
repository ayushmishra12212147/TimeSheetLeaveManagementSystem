using ReportService.Enums;

namespace ReportService.Models
{
    public class ReportRequest
    {
        public Guid Id { get; set; }
        public ReportType ReportType { get; set; }
        public ReportRequestStatus Status { get; set; }
        public Guid RequestedByUserId { get; set; }
        public string RequestedByEmployeeId { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public string? ScopeEmployeeId { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public Guid? ApprovedByUserId { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public Guid? RejectedByUserId { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
    }
}
