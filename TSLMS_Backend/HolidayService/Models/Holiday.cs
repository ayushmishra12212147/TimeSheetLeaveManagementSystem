namespace HolidayService.Models
{
    public class Holiday
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly HolidayDate { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
