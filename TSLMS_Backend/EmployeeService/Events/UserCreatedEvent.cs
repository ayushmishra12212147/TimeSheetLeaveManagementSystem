namespace EmployeeService.Events
{
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string EmployeeId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string TempPassword { get; set; }
    }
}
