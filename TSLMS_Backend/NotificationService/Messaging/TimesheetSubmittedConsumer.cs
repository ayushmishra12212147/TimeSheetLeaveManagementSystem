using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class TimesheetSubmittedConsumer : RabbitMqConsumerBase<TimesheetSubmittedEvent>
    {
        public TimesheetSubmittedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TimesheetSubmittedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "timesheet.events";
        protected override string QueueName => "q.notification.timesheet-submitted";
        protected override string RoutingKey => "timesheet.submitted";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            TimesheetSubmittedEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleTimesheetSubmittedAsync(message, cancellationToken);
        }
    }
}
