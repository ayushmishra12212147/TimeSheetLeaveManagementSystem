namespace AuditService.DTOs
{
    public class AuditLogFilterDto
    {
        public string? ServiceName { get; set; }
        public string? EventKey { get; set; }
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public string? EntityId { get; set; }
        public Guid? ActorUserId { get; set; }
        public Guid? SubjectUserId { get; set; }
        public DateTime? DateFromUtc { get; set; }
        public DateTime? DateToUtc { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
