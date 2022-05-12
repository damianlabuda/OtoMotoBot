using Shared.Models;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Sender
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _iserviceScopeFactory;
        
        public Worker(ILogger<Worker> logger, IServiceScopeFactory iserviceScopeFactory)
        {
            _logger = logger;
            _iserviceScopeFactory = iserviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MessagesToSent messagesToSent = new MessagesToSent();

            TelegramSender sender = new TelegramSender(messagesToSent.Users, messagesToSent.NewAdMessages, _iserviceScopeFactory);
            await sender.SendsAsync();
        }
    }
}