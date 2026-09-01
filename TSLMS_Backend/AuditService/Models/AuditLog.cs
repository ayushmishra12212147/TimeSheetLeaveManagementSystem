using System.ComponentModel.DataAnnotations;

namespace AuditService.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime OccurredAtUtc { get; set; }

        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string EventKey { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EntityId { get; set; } = string.Empty;

        public Guid? ActorUserId { get; set; }

        [MaxLength(20)]
        public string? ActorEmployeeId { get; set; }

        [MaxLength(100)]
        public string? ActorName { get; set; }

        public Guid? SubjectUserId { get; set; }

        [MaxLength(20)]
        public string? SubjectEmployeeId { get; set; }

        [MaxLength(100)]
        public string? SubjectName { get; set; }

        [MaxLength(50)]
        public string Outcome { get; set; } = "Success";

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string MetadataJson { get; set; } = "{}";
        public DateTime CreatedAtUtc { get; set; }
    }
}
