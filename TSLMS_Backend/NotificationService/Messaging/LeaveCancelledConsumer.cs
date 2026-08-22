using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class LeaveCancelledConsumer : RabbitMqConsumerBase<LeaveCancelledEvent>
    {
        public LeaveCancelledConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LeaveCancelledConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "leave.events";
        protected override string QueueName => "q.notification.leave-cancelled";
        protected override string RoutingKey => "leave.cancelled";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            LeaveCancelledEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleLeaveCancelledAsync(message, cancellationToken);
        }
    }
}
