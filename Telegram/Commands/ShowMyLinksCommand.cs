using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Services;

namespace Telegram.Commands
{
    public class ShowMyLinksCommand : BaseCommand
    {
        private readonly IUserService _userService;
        private readonly TelegramBotClient _telegramBot;

        public ShowMyLinksCommand(TelegramBot telegramBot, IUserService userService)
        {
            _userService = userService;
            _telegramBot = telegramBot.GetBot().Result;
        }

        public override string Name => CommandNames.ShowMyLinksCommand;
        public override async Task ExecuteAsync(Update update)
        {
            await _telegramBot.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

            var user = await _userService.Get(update);

            if (user == null || user.SearchLinks.Count == 0)
            {
                await _telegramBot.AnswerCallbackQueryAsync(update.CallbackQuery.Id,
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

            await _telegramBot.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId, "Twoje linki wyszukiwania:",
                replyMarkup: inlineKeyboard);
        }
    }
}
