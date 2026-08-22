using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;

namespace EmployeeService.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _config;

        public RabbitMQPublisher(IConfiguration config)
        {
            _config = config;
        }

        public void Publish<T>(T message, string routingKey)
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"],
                Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var exchange = ResolveExchange(routingKey);

            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body
            );
        }

        private string ResolveExchange(string routingKey)
        {
            if (routingKey.StartsWith("attendance.", StringComparison.OrdinalIgnoreCase))
            {
                return _config["RabbitMQ:AttendanceExchange"] ?? "attendance.events";
            }

            return _config["RabbitMQ:Exchange"] ?? "user.events";
        }
    }
}
