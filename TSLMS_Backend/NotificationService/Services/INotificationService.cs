using NotificationService.DTOs;

namespace NotificationService.Services
{
    public interface INotificationService
    {
        Task<NotificationListResponseDto> GetNotificationsAsync(Guid userId, NotificationQueryDto query, CancellationToken cancellationToken);
        Task MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
        Task<UnreadCountDto> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken);
        Task<NotificationPreferenceDto> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken);
        Task<NotificationPreferenceDto> UpdatePreferencesAsync(Guid userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<NotificationTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken);
        Task<NotificationTemplateDto> UpdateTemplateAsync(Guid id, UpdateNotificationTemplateDto dto, CancellationToken cancellationToken);
    }
}
