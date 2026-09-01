namespace NotificationService.DTOs
{
    public class NotificationListResponseDto
    {
        public IReadOnlyCollection<NotificationResponseDto> Items { get; set; } = Array.Empty<NotificationResponseDto>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
