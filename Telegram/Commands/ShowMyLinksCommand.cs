using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Commands
{
    public class ShowMyLinksCommand : IBaseCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IUserService _userService;

        public ShowMyLinksCommand(ITelegramBotClient telegramBotClient, IUserService userService)
        {
            _telegramBotClient = telegramBotClient;
            _userService = userService;
        }

        public string Name => CommandNames.ShowMyLinksCommand;
        public async Task ExecuteAsync(Update update)
        {
            await _telegramBotClient.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

            var user = await _userService.Get(update);

            if (user == null || user.SearchLinks.Count == 0)
            {
                await _telegramBotClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id,
                    "Brak linków do wyświetlenia");
                return;
            }

            InlineKeyboardMarkup inlineKeyboard = user.SearchLinks.Select(x =>
                new[] { new InlineKeyboardButton(x.Link) { CallbackData = $"{CommandNames.OptionsLinkCommand}{x.Id}" } }).ToArray();

            inlineKeyboard = inlineKeyboard.InlineKeyboard.Concat((new[]
            {
                new[]
                {
                    new InlineKeyboardButton("Cofnij") {CallbackData = CommandNames.DefaultCommand}
                }
            })).ToArray();

            await _telegramBotClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId, "Twoje linki wyszukiwania:",
                replyMarkup: inlineKeyboard);
        }
    }
}
