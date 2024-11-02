using Mango.MessageBus;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.ShoppingCartAPI.RabbitMQSender
{
    public class RabbitMQCartMessageSender : IRabbitMQCartMessageSender
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private IConnection _connection;
        public RabbitMQCartMessageSender()
        {
            _hostname = "localhost";
            _password = "guest";
            _username = "guest";
        }
        public void Send(object message, string queueName)
        {
            if (ConnectionExist()) 
            {
                using var channel = _connection.CreateModel();
                channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);
                channel.BasicPublish(exchange:"",routingKey: queueName,basicProperties:null, body: body);
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
