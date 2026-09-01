using System.Text;
using AuditService.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuditService.Messaging
{
    public class AuditEventConsumer : BackgroundService
    {
        private static readonly (string Exchange, string RoutingKey)[] Subscriptions =
        [
            ("user.events", "user.created"),
            ("user.events", "user.manager-assignment.changed"),
            ("auth.events", "password.reset.requested"),
            ("leave.events", "leave.submitted"),
            ("leave.events", "leave.approved"),
            ("leave.events", "leave.rejected"),
            ("leave.events", "leave.cancelled"),
            ("timesheet.events", "timesheet.submitted"),
            ("timesheet.events", "timesheet.approved"),
            ("timesheet.events", "timesheet.rejected"),
            ("report.events", "report.requested"),
            ("report.events", "report.approved"),
            ("report.events", "report.rejected"),
            ("attendance.events", "attendance.clockin"),
            ("attendance.events", "attendance.clockout")
        ];

        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AuditEventConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        public AuditEventConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AuditEventConsumer> logger)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            const string queueName = "q.audit.events";
            _channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

            foreach (var (exchange, routingKey) in Subscriptions)
            {
                _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
                _channel.QueueBind(queueName, exchange, routingKey);
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, eventArgs) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    using var scope = _serviceScopeFactory.CreateScope();
                    var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();
                    await auditLogService.RecordEventAsync(eventArgs.RoutingKey, payload, stoppingToken);
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process audit event {RoutingKey}.", eventArgs.RoutingKey);
                    _channel?.BasicNack(eventArgs.DeliveryTag, false, false);
                }
            };

            _channel.BasicConsume(queueName, false, consumer);

            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
