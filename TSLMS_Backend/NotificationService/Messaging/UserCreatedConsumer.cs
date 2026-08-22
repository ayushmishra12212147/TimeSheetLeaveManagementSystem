using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class UserCreatedConsumer : RabbitMqConsumerBase<UserCreatedEvent>
    {
        public UserCreatedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<UserCreatedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "user.events";
        protected override string QueueName => "q.notification.user-created";
        protected override string RoutingKey => "user.created";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            UserCreatedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleUserCreatedAsync(message, cancellationToken);
        }
    }
}
