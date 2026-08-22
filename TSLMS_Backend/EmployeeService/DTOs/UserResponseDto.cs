namespace EmployeeService.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Gender { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ManagerId { get; set; }

        public DepartmentDto Department { get; set; }
    }
}
