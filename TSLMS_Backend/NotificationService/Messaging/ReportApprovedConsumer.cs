using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class ReportApprovedConsumer : RabbitMqConsumerBase<ReportApprovedEvent>
    {
        public ReportApprovedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ReportApprovedConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "report.events";
        protected override string QueueName => "q.notification.report-approved";
        protected override string RoutingKey => "report.approved";

        protected override Task ProcessMessageAsync(INotificationDispatchService dispatchService, ReportApprovedEvent message, CancellationToken cancellationToken)
        {
            return dispatchService.HandleReportApprovedAsync(message, cancellationToken);
        }
    }
}
