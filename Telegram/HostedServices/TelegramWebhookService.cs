using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Telegram.HostedServices
{
    public class TelegramWebhookService : IHostedService
    {
        private readonly ILogger<TelegramWebhookService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public TelegramWebhookService(ILogger<TelegramWebhookService> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            var webhookAddress =
                $"{_configuration.GetConnectionString("Url")}/api/telegram/update/{_configuration.GetConnectionString("TelegramToken")}";
            _logger.LogInformation($"Setting webhook: {webhookAddress}");

            await botClient.SetWebhookAsync(url: webhookAddress, allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

            _logger.LogInformation($"Removing webhook");
            await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
        }
    }
}
