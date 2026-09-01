namespace NotificationService.DTOs
{
    public class NotificationPreferenceDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool InAppNotificationsEnabled { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
