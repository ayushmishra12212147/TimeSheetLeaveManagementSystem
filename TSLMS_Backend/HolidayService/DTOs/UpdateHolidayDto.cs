namespace HolidayService.DTOs
{
    public class UpdateHolidayDto
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly HolidayDate { get; set; }
        public string? Description { get; set; }
    }
}
