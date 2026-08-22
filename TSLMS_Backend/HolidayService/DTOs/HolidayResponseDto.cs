namespace HolidayService.DTOs
{
    public class HolidayResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly HolidayDate { get; set; }
        public string? Description { get; set; }
        public int Year { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
