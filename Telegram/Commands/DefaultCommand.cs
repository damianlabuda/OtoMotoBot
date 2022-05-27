using Redis.OM;
using Redis.OM.Searching;
using Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands
{
    public class DefaultCommand : BaseCommand
    {
        private readonly TelegramBotClient _telegramBot;

        private readonly IRedisCollection<TelegramCurrentAction> _currentActions;

        private readonly ReplyKeyboardMarkup _keyboardButtonDefault = new(new[]
        {
            new KeyboardButton[] { "Pokaż moje linki", "Dodaj link" },
        })
        {
            ResizeKeyboard = true
        };

        public DefaultCommand(TelegramBot telegramBot, RedisConnectionProvider redis)
        {
            _telegramBot = telegramBot.GetBot().Result;
            _currentActions = redis.RedisCollection<TelegramCurrentAction>();
        }

        public override string Name => CommandNames.DefaultCommand;

        public override async Task ExecuteAsync(Update update)
        {
            string text =
                "Kanał służy do śledzenia nowo dodanych aukcji w serwisie otomoto.pl, " +
                "Aby otrzymywać takie powiadomienia, dodaj za pomocą przycisku poniżej " +
                "„Dodaj link” swój link wyszukiwania z interesującymi cię filtrami " +
                "np. marka, model, przebieg, cena, rocznik itp.\r\n\r\n" +
                "Aby uzyskać swój link wyszukiwania, należy wejść na stronę otomoto.pl, " +
                "wybrać kategorie/markę/model + zaznaczyć interesujące nas filtry, wyszukać " +
                "aukcje i skopiować wygenerowany link z adresu przeglądarki.\r\n\r\nPrzykładowy " +
                "link wyszukiwania dla: Volkswagen Passat od 2015 rok, Automat" +
                "\r\nhttps://www.otomoto.pl/osobowe/volkswagen/passat/od-2015?search%5Bfilter_enum_gearbox%5D=automatic";

            await _telegramBot.SendTextMessageAsync(update.Message.Chat.Id, text,
                ParseMode.Markdown, replyMarkup: _keyboardButtonDefault);

            // Set redis current action value
            await _currentActions.InsertAsync(new TelegramCurrentAction()
            {
                CurrentAction = CommandNames.DefaultCommand,
                TelegramChatId = update.Message.Chat.Id
            });
        }
    }
}
