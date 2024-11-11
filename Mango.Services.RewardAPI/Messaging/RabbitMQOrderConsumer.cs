
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Service;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Channels;

namespace Mango.Services.EmailAPI.Messaging
{
    public class RabbitMQOrderConsumer : BackgroundService
    {
        private IConnection _connection;
        private IModel _channel;
        private RewardService _rewardService;
        //fanout
        //private const string ExchangeName = "PublishSubscribeOrderCreated_Exchange";
        //private readonly string _queueName;

        //2-exchange of type direct with routing keys
        private const string ExchangeName = "DirectOrderCreated_Exchange";
        private const string DirectOrderRewardQueueName = "DirectOrderRewardQueueName";
        public RabbitMQOrderConsumer(RewardService rewardService)
        {
            this._rewardService = rewardService;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            //fanout
            //_channel.ExchangeDeclare(exchange: ExchangeName, ExchangeType.Fanout);
            //_queueName = _channel.QueueDeclare(queue: "OrderRewardqueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            //_channel.QueueBind(_queueName, ExchangeName,routingKey: "");

            //direct
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: false);
            _channel.QueueDeclare(queue: DirectOrderRewardQueueName, false, false, false);
            _channel.QueueBind(queue: DirectOrderRewardQueueName, exchange: ExchangeName, routingKey: "RewardOrderRK");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (channel, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                RewardMessage rewardMessage = JsonConvert.DeserializeObject<RewardMessage>(message);

                HandleMessage(rewardMessage).GetAwaiter().GetResult();
                //after handle message we want do delete message from queue
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };
            //fanout
            //_channel.BasicConsume(_queueName, false, consumer);

            //direct
            _channel.BasicConsume(DirectOrderRewardQueueName, false, consumer);
            return Task.CompletedTask;
        }
        private async Task HandleMessage(RewardMessage rewardMessage)
        {
            //TODO- try to log email
            await _rewardService.UpdateReward(rewardMessage);
        }
    }
}
