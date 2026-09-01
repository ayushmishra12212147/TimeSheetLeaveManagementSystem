using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class ReportRequestedConsumer : RabbitMqConsumerBase<ReportRequestedEvent>
    {
        public ReportRequestedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ReportRequestedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "report.events";
        protected override string QueueName => "q.notification.report-requested";
        protected override string RoutingKey => "report.requested";

        protected override Task ProcessMessageAsync(INotificationDispatchService dispatchService, ReportRequestedEvent message, CancellationToken cancellationToken)
        {
            return dispatchService.HandleReportRequestedAsync(message, cancellationToken);
        }
    }
}
