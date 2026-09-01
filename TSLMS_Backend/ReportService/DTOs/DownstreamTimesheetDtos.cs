namespace ReportService.DTOs
{
    public class DownstreamWeeklyTimesheetSummaryResponseDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeUserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public Guid? ManagerUserId { get; set; }
        public string? ManagerName { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLateSubmission { get; set; }
        public bool MeetsMinimumWeeklyHours { get; set; }
        public int EntryCount { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
    }
}
