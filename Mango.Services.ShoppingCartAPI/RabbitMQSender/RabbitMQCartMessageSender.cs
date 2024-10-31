using Mango.MessageBus;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        public RabbitMQCartMessageSender()
        {
            _hostname = "localhost";
            _hostname = "guest";
            _username = "guest";
        }
        public void Send(BaseMessage message, string queueName)
        {
            throw new NotImplementedException();
        }
    }
}
