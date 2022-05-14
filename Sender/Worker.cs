using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;
using System.Text;

namespace Sender
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _iserviceScopeFactory;
        private ConnectionFactory _connectionFactory;
        private IModel _channel;
        private IConnection _connection;
        
        public Worker(ILogger<Worker> logger, IServiceScopeFactory iserviceScopeFactory)
        {
            _logger = logger;
            _iserviceScopeFactory = iserviceScopeFactory;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true
            };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "messagesToSend", durable: false, exclusive: false, autoDelete: true, arguments: null);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (sender, args) =>
            {
                var message = Encoding.UTF8.GetString(args.Body.ToArray());

                var messagesToSent = JsonConvert.DeserializeObject<MessagesToSent>(message);

                var telegramSender = new TelegramSender(messagesToSent.Users, messagesToSent.NewAdMessages, _iserviceScopeFactory);
                await telegramSender.SendsAsync();

            };
            _channel.BasicConsume(queue: "messagesToSend", autoAck: true, consumer: consumer);

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }
    }
}