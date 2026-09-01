using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class TimesheetRejectedConsumer : RabbitMqConsumerBase<TimesheetRejectedEvent>
    {
        public TimesheetRejectedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TimesheetRejectedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "timesheet.events";
        protected override string QueueName => "q.notification.timesheet-rejected";
        protected override string RoutingKey => "timesheet.rejected";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            TimesheetRejectedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleTimesheetRejectedAsync(message, cancellationToken);
        }
    }
}
