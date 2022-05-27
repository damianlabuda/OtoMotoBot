using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands
{
    public class ShowMyLinksCommand : BaseCommand
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly TelegramBotClient _telegramBot;

        public ShowMyLinksCommand(OtoMotoContext otoMotoContext, TelegramBot telegramBot)
        {
            _otoMotoContext = otoMotoContext;
            _telegramBot = telegramBot.GetBot().Result;
        }

        public override string Name => CommandNames.ShowMyLinksCommand;
        public override async Task ExecuteAsync(Update update)
        {
            var user = await _otoMotoContext.Users.Include(x => x.SearchLinks)
                .FirstOrDefaultAsync(x => x.TelegramChatId == update.Message.Chat.Id);

            if (user == null)
            {
                await _telegramBot.SendTextMessageAsync(update.Message.Chat.Id, "Brak linków do wyświetlenia");
                return;
            }

            InlineKeyboardMarkup inlineKeyboard = user.SearchLinks.Select(x => new InlineKeyboardButton(x.Link){ CallbackData = x.Id.ToString() }).ToArray();

            if (!inlineKeyboard.InlineKeyboard.Any())
            {
                await _telegramBot.SendTextMessageAsync(update.Message.Chat.Id, "Brak linków do wyświetlenia");
                return;
            }

            await _telegramBot.SendTextMessageAsync(update.Message.Chat.Id, "Twoje zapisane linki wyszukiwania:",
                ParseMode.Markdown, replyMarkup: inlineKeyboard);
        }
    }
}
