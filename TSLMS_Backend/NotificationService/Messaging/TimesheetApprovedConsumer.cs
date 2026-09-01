using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class TimesheetApprovedConsumer : RabbitMqConsumerBase<TimesheetApprovedEvent>
    {
        public TimesheetApprovedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TimesheetApprovedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "timesheet.events";
        protected override string QueueName => "q.notification.timesheet-approved";
        protected override string RoutingKey => "timesheet.approved";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            TimesheetApprovedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleTimesheetApprovedAsync(message, cancellationToken);
        }
    }
}
