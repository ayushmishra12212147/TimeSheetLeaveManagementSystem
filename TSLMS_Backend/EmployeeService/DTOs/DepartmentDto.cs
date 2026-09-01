namespace EmployeeService.DTOs
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<UserResponseDto> Users { get; set; }
    }
    
}
