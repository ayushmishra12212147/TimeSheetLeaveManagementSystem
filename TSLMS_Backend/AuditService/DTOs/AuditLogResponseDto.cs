namespace AuditService.DTOs
{
    public class AuditLogResponseDto
    {
        public Guid Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string EventKey { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public Guid? ActorUserId { get; set; }
        public string? ActorEmployeeId { get; set; }
        public string? ActorName { get; set; }
        public Guid? SubjectUserId { get; set; }
        public string? SubjectEmployeeId { get; set; }
        public string? SubjectName { get; set; }
        public string Outcome { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MetadataJson { get; set; } = "{}";
    }
}
