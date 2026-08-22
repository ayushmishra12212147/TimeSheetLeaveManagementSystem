using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class LeaveRejectedConsumer : RabbitMqConsumerBase<LeaveRejectedEvent>
    {
        public LeaveRejectedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LeaveRejectedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "leave.events";
        protected override string QueueName => "q.notification.leave-rejected";
        protected override string RoutingKey => "leave.rejected";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            LeaveRejectedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleLeaveRejectedAsync(message, cancellationToken);
        }
    }
}
