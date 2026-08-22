using LeaveService.Enums;

namespace LeaveService.DTOs
{
    public class CreateLeaveRequestDto
    {
        public Guid LeaveTypeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public bool IsHalfDay { get; set; }
        public HalfDaySession HalfDaySession { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? SupportingDocumentUrl { get; set; }
    }
}
