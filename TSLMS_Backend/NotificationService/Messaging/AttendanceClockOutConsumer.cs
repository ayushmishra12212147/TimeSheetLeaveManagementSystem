using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class AttendanceClockOutConsumer : RabbitMqConsumerBase<AttendanceClockOutEvent>
    {
        public AttendanceClockOutConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AttendanceClockOutConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "attendance.events";
        protected override string QueueName => "q.notification.attendance-clockout";
        protected override string RoutingKey => "attendance.clockout";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            AttendanceClockOutEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleAttendanceClockOutAsync(message, cancellationToken);
        }
    }
}
