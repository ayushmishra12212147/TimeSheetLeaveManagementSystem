namespace LeaveService.DTOs
{
    public class EmployeeDirectoryUserDto
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }
    }
}
