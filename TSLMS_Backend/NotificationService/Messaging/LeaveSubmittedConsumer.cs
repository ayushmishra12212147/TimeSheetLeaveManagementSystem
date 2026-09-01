using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class LeaveSubmittedConsumer : RabbitMqConsumerBase<LeaveSubmittedEvent>
    {
        public LeaveSubmittedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LeaveSubmittedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "leave.events";
        protected override string QueueName => "q.notification.leave-submitted";
        protected override string RoutingKey => "leave.submitted";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            LeaveSubmittedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleLeaveSubmittedAsync(message, cancellationToken);
        }
    }
}
