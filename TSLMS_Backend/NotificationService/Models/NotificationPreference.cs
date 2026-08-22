namespace NotificationService.Models
{
    public class NotificationPreference
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool InAppNotificationsEnabled { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
