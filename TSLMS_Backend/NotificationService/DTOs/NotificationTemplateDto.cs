namespace NotificationService.DTOs
{
    public class NotificationTemplateDto
    {
        public Guid Id { get; set; }
        public string EventKey { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string SubjectTemplate { get; set; } = string.Empty;
        public string BodyTemplate { get; set; } = string.Empty;
        public bool IsCritical { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
