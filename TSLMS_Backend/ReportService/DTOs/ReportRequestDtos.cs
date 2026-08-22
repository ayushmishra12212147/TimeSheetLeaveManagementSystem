using ReportService.Enums;

namespace ReportService.DTOs
{
    public class CreateReportRequestDto
    {
        public ReportType ReportType { get; set; }
        public string? EmployeeId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
    }

    public class RejectReportRequestDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class ReportRequestResponseDto
    {
        public Guid Id { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string RequestedByEmployeeId { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public string? ScopeEmployeeId { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
    }
}
