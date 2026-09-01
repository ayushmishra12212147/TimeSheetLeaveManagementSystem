namespace LeaveService.DTOs
{
    public class HolidayCheckResponseDto
    {
        public DateOnly Date { get; set; }
        public bool IsHoliday { get; set; }
        public Guid? HolidayId { get; set; }
        public string? HolidayName { get; set; }
    }
}
