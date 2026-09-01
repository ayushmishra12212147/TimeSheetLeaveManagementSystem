namespace AuthService.DTOs
{
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
        public bool IsFirstLogin { get; set; }
        public bool MustResetPassword { get; set; }
        public bool IsProfileComplete { get; set; }
    }
}
