using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class PasswordResetRequestedConsumer : RabbitMqConsumerBase<PasswordResetRequestedEvent>
    {
        public PasswordResetRequestedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<PasswordResetRequestedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "auth.events";
        protected override string QueueName => "q.notification.password-reset-requested";
        protected override string RoutingKey => "password.reset.requested";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            PasswordResetRequestedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandlePasswordResetRequestedAsync(message, cancellationToken);
        }
    }
}
