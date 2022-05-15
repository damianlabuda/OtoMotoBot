using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Shared.Entities;
using Shared.Models;
using System.Text;

namespace Scraper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IOtoMotoHttpClient _otoMotoHttpClient;

        private ConnectionFactory _connectionFactory;

        private IConnection _connection;

        private IModel _channelSearchLinks;

        private IModel _channelMessagesToSend;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IOtoMotoHttpClient otoMotoHttpClient)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _otoMotoHttpClient = otoMotoHttpClient;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory()
            {
                DispatchConsumersAsync = true
            };
            _connection = _connectionFactory.CreateConnection();

            _channelSearchLinks = _connection.CreateModel();
            _channelSearchLinks.QueueDeclare(queue: "searchLinks", durable: false, exclusive: false, autoDelete: false, arguments: null);

            _channelMessagesToSend = _connection.CreateModel();
            _channelMessagesToSend.QueueDeclare(queue: "messagesToSend", durable: false, exclusive: false, autoDelete: false, arguments: null);

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channelSearchLinks);

            consumer.Received += async (sender, args) =>
            {
                var receivedMessage = Encoding.UTF8.GetString(args.Body.ToArray());

                try
                {
                    var searchLink = JsonConvert.DeserializeObject<SearchLink>(receivedMessage);

                    var otomotoSearch = new OtomotoSearchAuctions(searchLink, _otoMotoHttpClient);
                    var adLinks = await otomotoSearch.Search();

                    if (adLinks.Any())
                    {
                        var searchInDb = new SearchInDb(_serviceScopeFactory);
                        var newAdMessages = await searchInDb.Check(adLinks);

                        if (newAdMessages.Any() && searchLink.SearchCount > 0)
                        {
                            MessagesToSent messagesToSent = new MessagesToSent()
                            {
                                NewAdMessages = newAdMessages,
                                //Users = searchLink.Users.Select(x => new User { TelegramChatId = x.TelegramChatId }).ToList<User>()
                                Users = searchLink.Users
                            };

                            var jsonMessagesToSent = JsonConvert.SerializeObject(messagesToSent);
                            var body = Encoding.UTF8.GetBytes(jsonMessagesToSent);

                            _channelMessagesToSend.BasicPublish(exchange: "", "messagesToSend", basicProperties: null, body);
                        }
                    }
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

            _channelSearchLinks.BasicConsume(queue: "searchLinks", autoAck: true, consumer: consumer);

            await Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _connection.Close();
        }
    }
}