using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;
using System.Text;
using RabbitMQ.Client.Exceptions;

namespace Sender
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _iserviceScopeFactory;

        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IModel _channelMessagesToSend;

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
            _channelMessagesToSend = _connection.CreateModel();
            _channelMessagesToSend.QueueDeclare(queue: "messagesToSend", durable: false, exclusive: false, autoDelete: false, arguments: null);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channelMessagesToSend);

            consumer.Received += async (sender, args) =>
            {
                var receivedMessage = Encoding.UTF8.GetString(args.Body.ToArray());

                try
                {
                    var messagesToSent = JsonConvert.DeserializeObject<MessagesToSent>(receivedMessage);
                    var telegramSender = new TelegramSender(messagesToSent.Users, messagesToSent.NewAdMessages,
                        _iserviceScopeFactory);
                    var feedback = await telegramSender.SendsAsync();
                    _logger.LogInformation(feedback);
                }
                catch (JsonException e)
                {
                    _logger.LogError($"Json parse error: {e.Message}, json string: {receivedMessage}");
                }
                catch (AlreadyClosedException)
                {
                    _logger.LogError($"Problem with RabbitMQ connection");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Exception: {e.Message}");
                }
            };

            _channelMessagesToSend.BasicConsume(queue: "messagesToSend", autoAck: true, consumer: consumer);

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }
    }
}