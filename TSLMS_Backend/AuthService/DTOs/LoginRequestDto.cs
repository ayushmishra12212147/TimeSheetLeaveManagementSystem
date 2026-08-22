namespace AuthService.DTOs
{
    public class LoginRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
