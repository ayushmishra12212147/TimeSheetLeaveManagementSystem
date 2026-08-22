namespace EmployeeService.Messaging
{
    public interface IRabbitMQPublisher
    {
        void Publish<T>(T message, string routingKey);
    }
}