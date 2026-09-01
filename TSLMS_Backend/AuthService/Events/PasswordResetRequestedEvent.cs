namespace AuthService.Events
{
    public class PasswordResetRequestedEvent
    {
        public Guid UserId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string ResetToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
