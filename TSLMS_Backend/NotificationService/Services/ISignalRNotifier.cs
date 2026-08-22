using NotificationService.DTOs;

namespace NotificationService.Services
{
    public interface ISignalRNotifier
    {
        Task PushToUserAsync(Guid userId, RealtimeNotificationDto notification, CancellationToken cancellationToken);
    }
}
