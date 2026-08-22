using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificationService.Services;

namespace NotificationService.Messaging
{
    public abstract class RabbitMqConsumerBase<TMessage> : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger _logger;
        private IConnection? _connection;
        private IModel? _channel;

        protected RabbitMqConsumerBase(
            IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            ILogger logger)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected abstract string ExchangeName { get; }
        protected abstract string QueueName { get; }
        protected abstract string RoutingKey { get; }

        protected abstract Task ProcessMessageAsync(
            INotificationDispatchService dispatchService,
            TMessage message,
            CancellationToken cancellationToken);

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

            _channel.ExchangeDeclare(ExchangeName, ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(QueueName, ExchangeName, RoutingKey);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, eventArgs) =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                    var message = JsonSerializer.Deserialize<TMessage>(payload, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (message == null)
                    {
                        throw new InvalidOperationException("Message payload could not be deserialized.");
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var dispatchService = scope.ServiceProvider.GetRequiredService<INotificationDispatchService>();
                    await ProcessMessageAsync(dispatchService, message, stoppingToken);
                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process RabbitMQ message for queue {QueueName}.", QueueName);
                    _channel?.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);

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
