namespace HolidayService.Helpers
{
    public class ApiResponse<T>
    {
        public ApiResponse(T? data, string message, bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public bool Success { get; init; }
        public string Message { get; init; }
        public T? Data { get; init; }
    }
}
