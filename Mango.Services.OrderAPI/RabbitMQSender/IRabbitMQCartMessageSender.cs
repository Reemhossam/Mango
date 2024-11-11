namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public interface IRabbitMQCartMessageSender
    {
        //void Send(BaseMessage message, string queueName);
        void Send(object message);
    }
}
