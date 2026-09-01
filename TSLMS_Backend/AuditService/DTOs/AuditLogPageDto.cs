namespace AuditService.DTOs
{
    public class AuditLogPageDto
    {
        public IReadOnlyCollection<AuditLogResponseDto> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
