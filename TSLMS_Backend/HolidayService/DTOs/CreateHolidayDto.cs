namespace HolidayService.DTOs
{
    public class CreateHolidayDto
    {
        public string Name { get; set; } = string.Empty;
        public DateOnly HolidayDate { get; set; }
        public string? Description { get; set; }
    }
}
