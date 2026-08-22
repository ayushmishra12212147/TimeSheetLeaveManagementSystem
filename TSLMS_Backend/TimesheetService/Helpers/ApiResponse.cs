namespace TimesheetService.Helpers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(T data, string message = "Success")
        {
            Data = data;
            Message = message;
        }

        public ApiResponse(string message)
        {
            Message = message;
            Data = default!;
        }
    }
}
