using Telegram.Bot;

namespace Telegram
{
    public class TelegramBot
    {
        private readonly IConfiguration _configuration;
        private TelegramBotClient _telegramBotClient;

        public TelegramBot(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<TelegramBotClient> GetBot()
        {
            if (_telegramBotClient != null)
                return _telegramBotClient;

            _telegramBotClient = new TelegramBotClient(_configuration.GetConnectionString("TelegramToken"));

            var hook = $"{_configuration.GetConnectionString("Url")}/api/telegram/update";

            await _telegramBotClient.SetWebhookAsync(hook);

            return _telegramBotClient;
        }
    }
}
