using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.OrderAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        //1-exchange of type fanout
        //private const string ExchangeName = "PublishSubscribeOrderCreated_Exchange";   
        
        //2-exchange of type direct with routing keys
        private const string ExchangeName = "DirectOrderCreated_Exchange";
        private const string DirectOrderEmailQueueName = "DirectOrderEmailQueueName";
        private const string DirectOrderRewardQueueName = "DirectOrderRewardQueueName";
        public RabbitMQCartMessageSender()
        {
            _hostname = "localhost";
            _password = "guest";
            _username = "guest";
        }
        public void Send(object message)
        {
            if (ConnectionExist()) 
            {
                using var channel = _connection.CreateModel();
                //fanout
                //channel.ExchangeDeclare(exchange:ExchangeName, type:ExchangeType.Fanout, durable:false);

                //direct
                channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: false);
                channel.QueueDeclare(queue:DirectOrderEmailQueueName,false,false,false);
                channel.QueueDeclare(queue: DirectOrderRewardQueueName, false, false, false);
                channel.QueueBind(queue: DirectOrderEmailQueueName, exchange: ExchangeName, routingKey: "EmailOrderRK");
                channel.QueueBind(queue: DirectOrderRewardQueueName, exchange: ExchangeName, routingKey: "RewardOrderRK");

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                //fanout
                //channel.BasicPublish(exchange:ExchangeName,routingKey: "",basicProperties:null, body: body);
                
                //direct
                channel.BasicPublish(exchange: ExchangeName, routingKey: "EmailOrderRK", basicProperties: null, body: body);
                channel.BasicPublish(exchange: ExchangeName, routingKey: "RewardOrderRK", basicProperties: null, body: body);
            }
        }
        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };
                _connection = factory.CreateConnection();
            }
            catch (Exception ex) 
            { 
                //log error message
            }

        }
        private bool ConnectionExist()
        {
            if (_connection != null) return true;
            else
            {
                CreateConnection();
                return _connection != null;
            }

        }
    }
}
