using TimesheetService.Enums;

namespace TimesheetService.DTOs
{
    public class CreateTimesheetEntryDto
    {
        public DateOnly EntryDate { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Hours { get; set; }
        public TimesheetCategory Category { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateTimesheetEntryDto
    {
        public DateOnly EntryDate { get; set; }
        public Guid ProjectId { get; set; }
        public decimal Hours { get; set; }
        public TimesheetCategory Category { get; set; }
        public string? Description { get; set; }
    }

    public class SubmitTimesheetDto
    {
        public DateOnly? WeekStartDate { get; set; }
    }

    public class ApproveTimesheetDto
    {
        public string? Comment { get; set; }
    }

    public class RejectTimesheetDto
    {
        public string Reason { get; set; } = string.Empty;
    }

    public class TimesheetEntryResponseDto
    {
        public Guid Id { get; set; }
        public Guid WeeklyTimesheetSummaryId { get; set; }
        public DateOnly EntryDate { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal Hours { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal DailyTotalHours { get; set; }
        public bool IsBelowDailyThresholdWarning { get; set; }
        public bool IsAboveDailyThresholdWarning { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public class WeeklyTimesheetSummaryResponseDto
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

    public class WeekTimesheetResponseDto
    {
        public Guid? SummaryId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public DateOnly WeekStartDate { get; set; }
        public DateOnly WeekEndDate { get; set; }
        public decimal TotalHours { get; set; }
        public decimal MinimumWeeklyHours { get; set; }
        public bool MeetsMinimumWeeklyHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsLateSubmission { get; set; }
        public string? RejectionReason { get; set; }
        public IReadOnlyCollection<TimesheetEntryResponseDto> Entries { get; set; } = Array.Empty<TimesheetEntryResponseDto>();
    }
}
