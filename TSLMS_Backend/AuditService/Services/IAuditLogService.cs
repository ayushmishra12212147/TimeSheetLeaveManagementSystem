using AuditService.DTOs;

namespace AuditService.Services
{
    public interface IAuditLogService
    {
        Task RecordEventAsync(string eventKey, string payloadJson, CancellationToken cancellationToken = default);
        Task<AuditLogPageDto> GetAsync(AuditLogFilterDto filter, CancellationToken cancellationToken = default);
        Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
    }
}
