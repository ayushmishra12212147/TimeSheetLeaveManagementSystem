using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class LeaveApprovedConsumer : RabbitMqConsumerBase<LeaveApprovedEvent>
    {
        public LeaveApprovedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<LeaveApprovedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "leave.events";
        protected override string QueueName => "q.notification.leave-approved";
        protected override string RoutingKey => "leave.approved";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            LeaveApprovedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleLeaveApprovedAsync(message, cancellationToken);
        }
    }
}
