using NotificationService.Enums;

namespace NotificationService.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid RecipientUserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReadAtUtc { get; set; }
        public string? ActionUrl { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public bool IsImportant { get; set; }
    }
}
