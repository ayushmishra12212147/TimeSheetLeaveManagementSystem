namespace HolidayService.DTOs
{
    public class CopyHolidayYearDto
    {
        public int SourceYear { get; set; }
        public int TargetYear { get; set; }
        public bool SkipExistingDates { get; set; } = true;
    }
}
