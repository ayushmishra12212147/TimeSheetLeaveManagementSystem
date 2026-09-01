using Microsoft.AspNetCore.SignalR;
using NotificationService.DTOs;
using NotificationService.Hubs;

namespace NotificationService.Services
{
    public class SignalRNotifier : ISignalRNotifier
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotifier(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PushToUserAsync(Guid userId, RealtimeNotificationDto notification, CancellationToken cancellationToken)
        {
            return _hubContext.Clients
                .Group($"user-{userId}")
                .SendAsync("notification", notification, cancellationToken);
        }
    }
}
