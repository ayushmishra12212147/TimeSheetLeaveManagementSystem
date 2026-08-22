using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class ReportRejectedConsumer : RabbitMqConsumerBase<ReportRejectedEvent>
    {
        public ReportRejectedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ReportRejectedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "report.events";
        protected override string QueueName => "q.notification.report-rejected";
        protected override string RoutingKey => "report.rejected";

        protected override Task ProcessMessageAsync(INotificationDispatchService dispatchService, ReportRejectedEvent message, CancellationToken cancellationToken)
        {
            return dispatchService.HandleReportRejectedAsync(message, cancellationToken);
        }
    }
}
