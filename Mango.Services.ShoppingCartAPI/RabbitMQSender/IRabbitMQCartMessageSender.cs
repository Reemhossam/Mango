
using Mango.MessageBus;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public interface IRabbitMQCartMessageSender
    {
        //void Send(BaseMessage message, string queueName);
        void Send(object message, string queueName);
    }
}
