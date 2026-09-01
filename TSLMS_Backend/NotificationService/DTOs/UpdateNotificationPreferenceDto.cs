namespace NotificationService.DTOs
{
    public class UpdateNotificationPreferenceDto
    {
        public bool EmailNotificationsEnabled { get; set; }
        public bool InAppNotificationsEnabled { get; set; }
    }
}
