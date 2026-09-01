using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TimesheetService.Data;
using TimesheetService.Events;

namespace TimesheetService.Messaging
{
    public class ManagerAssignmentChangedConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ManagerAssignmentChangedConsumer> _logger;
        private IConnection? _connection;
        private IModel? _channel;

        public ManagerAssignmentChangedConsumer(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<ManagerAssignmentChangedConsumer> logger)
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

            _channel.ExchangeDeclare("user.events", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare("q.timesheet.manager-assignment-changed", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind("q.timesheet.manager-assignment-changed", "user.events", "user.manager-assignment.changed");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, eventArgs) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    var message = JsonSerializer.Deserialize<ManagerAssignmentChangedEvent>(payload, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (message == null)
                    {
                        throw new InvalidOperationException("Message payload could not be deserialized.");
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TimesheetDbContext>();

                    var summaries = await dbContext.WeeklyTimesheetSummaries
                        .Where(x => x.EmployeeUserId == message.EmployeeUserId)
                        .ToListAsync(stoppingToken);

                    foreach (var summary in summaries)
                    {
                        summary.ManagerUserId = message.CurrentManagerUserId;
                        summary.ManagerName = message.CurrentManagerName;
                        summary.ManagerEmail = message.CurrentManagerEmail;
                        summary.UpdatedAtUtc = DateTime.UtcNow;
                    }

                    if (summaries.Count > 0)
                    {
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    _channel?.BasicAck(eventArgs.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process manager assignment change for timesheets.");
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: "q.timesheet.manager-assignment-changed", autoAck: false, consumer: consumer);

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
