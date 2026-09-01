using NotificationService.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public class AttendanceClockInConsumer : RabbitMqConsumerBase<AttendanceClockInEvent>
    {
        public AttendanceClockInConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AttendanceClockInConsumer> logger)
            : base(configuration, serviceScopeFactory, logger)
        {
        }

        protected override string ExchangeName => "attendance.events";
        protected override string QueueName => "q.notification.attendance-clockin";
        protected override string RoutingKey => "attendance.clockin";

        protected override Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            AttendanceClockInEvent message,
            CancellationToken cancellationToken)
        {
            return dispatchService.HandleAttendanceClockInAsync(message, cancellationToken);
        }
    }
}
