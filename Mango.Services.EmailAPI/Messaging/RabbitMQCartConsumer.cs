using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Service;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQCartConsumer : BackgroundService 
    {
        private IConnection _connection;
        private IModel _channel;
        private EmailService _emailService;
        public RabbitMQCartConsumer(EmailService emailService)
        {
            this._emailService = emailService;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "Checkoutqueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(message);

                HandleMessage(cartDto).GetAwaiter().GetResult();
                //after handle message we want do delete message from queue
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            _channel.BasicConsume("Checkoutqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task HandleMessage(CartDto cartDto) 
        { 
            //TODO- try to log email
            await _emailService.EmailCartAndLog(cartDto);
        }
    }
}
