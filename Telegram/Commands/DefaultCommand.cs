using Redis.OM;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Commands
{ 
    public class DefaultCommand : IBaseCommand, IDefaultCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly RedisConnectionProvider _redis;

        public DefaultCommand(ITelegramBotClient telegramBotClient, RedisConnectionProvider redis)
        {
            _telegramBotClient = telegramBotClient;
            _redis = redis;
        }

        public string Name => CommandNames.DefaultCommand;

        public async Task ExecuteAsync(Update update)
        {
            if (update.Type == UpdateType.Message)
                await _telegramBotClient.SendChatActionAsync(update.Message.Chat.Id, ChatAction.Typing);

            if (update.Type == UpdateType.CallbackQuery)
                await _telegramBotClient.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

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

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    new InlineKeyboardButton("Pokaż moje linki") { CallbackData = CommandNames.ShowMyLinksCommand },
                    new InlineKeyboardButton("Dodaj link") { CallbackData = CommandNames.AddLinkCommand }
                }
            });

            if (update.Type == UpdateType.Message)
            {
                await _telegramBotClient.SendTextMessageAsync(update.Message.Chat.Id, text,
                    ParseMode.Markdown, replyMarkup: inlineKeyboard);
                await _redis.Connection.UnlinkAsync($"TelegramCurrentAction:{update.Message.Chat.Id}");
                return;
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                await _telegramBotClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                    update.CallbackQuery.Message.MessageId, text, replyMarkup: inlineKeyboard);
                await _redis.Connection.UnlinkAsync($"TelegramCurrentAction:{update.CallbackQuery.Message.Chat.Id}");
                return;
            }
        }
    }
}
