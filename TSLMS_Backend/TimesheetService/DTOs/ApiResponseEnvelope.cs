namespace TimesheetService.DTOs
{
    public class ApiResponseEnvelope<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
    }
}
