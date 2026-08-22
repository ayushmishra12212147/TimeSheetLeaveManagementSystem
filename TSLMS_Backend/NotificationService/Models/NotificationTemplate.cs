using NotificationService.Enums;

namespace NotificationService.Models
{
    public class NotificationTemplate
    {
        public Guid Id { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public NotificationChannel Channel { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SubjectTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
