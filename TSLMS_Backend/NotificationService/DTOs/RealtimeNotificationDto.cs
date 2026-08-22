namespace NotificationService.DTOs
{
    public class RealtimeNotificationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsImportant { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? ActionUrl { get; set; }
    }
}
