namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public interface IRabbitMQAuthMessageSender
    {
        void Send(object message, string queueName);
    }
}
