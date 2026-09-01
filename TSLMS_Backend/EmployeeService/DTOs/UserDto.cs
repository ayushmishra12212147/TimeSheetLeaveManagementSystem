namespace EmployeeService.DTOs
{
    public class UserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string? Gender { get; set; }
        public Guid? DepartmentId { get; set; }
    }


}
