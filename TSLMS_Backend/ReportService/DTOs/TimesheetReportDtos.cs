namespace ReportService.DTOs
{
    public class TimesheetReportRequestDto
    {
        public string? EmployeeId { get; set; }
        public DateOnly? DateFrom { get; set; }
        public DateOnly? DateTo { get; set; }
        public string? Status { get; set; }
    }

    public class TimesheetReportRowDto
    {
        public Guid SummaryId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string? ManagerName { get; set; }
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public int EntryCount { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLateSubmission { get; set; }
        public bool MeetsMinimumWeeklyHours { get; set; }
        public DateTime? SubmittedAtUtc { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedByName { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
    }

    public class TimesheetReportSummaryDto
    {
        public int TotalWeeks { get; set; }
        public decimal TotalHours { get; set; }
        public decimal AverageHoursPerWeek { get; set; }
        public int ApprovedCount { get; set; }
        public int SubmittedCount { get; set; }
        public int RejectedCount { get; set; }
        public int DraftCount { get; set; }
        public int LateSubmissionCount { get; set; }
        public int MinimumHoursMetCount { get; set; }
    }

    public class TimesheetReportResponseDto
    {
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Scope { get; set; } = string.Empty;
        public IReadOnlyCollection<TimesheetReportRowDto> Rows { get; set; } = Array.Empty<TimesheetReportRowDto>();
        public TimesheetReportSummaryDto Summary { get; set; } = new();
    }
}
