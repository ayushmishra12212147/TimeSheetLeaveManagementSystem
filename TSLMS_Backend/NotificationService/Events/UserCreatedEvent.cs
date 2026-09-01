namespace NotificationService.Events
{
    public class UserCreatedEvent
    {
        public Guid UserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string TempPassword { get; set; } = string.Empty;
    }
}
