using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace AuthService.Messaging
{
    public class RabbitMQPublisher : IRabbitMQPublisher
    {
        private readonly IConfiguration _configuration;

        public RabbitMQPublisher(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Publish<T>(T message, string routingKey)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"]
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var exchange = _configuration["RabbitMQ:Exchange"] ?? "auth.events";
            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            channel.BasicPublish(exchange, routingKey, basicProperties: null, body);
        }
    }
}
