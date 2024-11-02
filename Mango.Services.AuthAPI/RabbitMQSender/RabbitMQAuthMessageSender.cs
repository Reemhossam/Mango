using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Mango.Services.AuthAPI.RabbitMQSender
{
    public class RabbitMQAuthMessageSender : IRabbitMQAuthMessageSender
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;
        IConnection _connection;
        public RabbitMQAuthMessageSender()
        {
            _host = "localhost";
            _username = "guest";
            _password = "guest";
        }
        public void Send(object message, string queueName)
        {
            if (ConnectionExist()) { 
            using var channel = _connection.CreateModel();
            channel.QueueDeclare(queueName,false,false,false,null);

            var josn = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(josn);

            channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
            }

        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _host,
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
