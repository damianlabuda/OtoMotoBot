using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Commands
{
    public class RemoveLinkCommand : BaseCommand
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly IDefaultCommand _defaultCommand;
        private readonly TelegramBotClient _telegramBot;

        public RemoveLinkCommand(OtoMotoContext otoMotoContext, TelegramBot telegramBot, IDefaultCommand defaultCommand)
        {
            _otoMotoContext = otoMotoContext;
            _defaultCommand = defaultCommand;
            _telegramBot = telegramBot.GetBot().Result;
        }
        public override string Name => CommandNames.RemoveLinkCommand;
        public override async Task ExecuteAsync(Update update)
        {
            await _telegramBot.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

            if (!Guid.TryParse(update.CallbackQuery.Data.Replace(CommandNames.RemoveLinkCommand, string.Empty), out Guid guidLinkId))
                return;

            var user = await _otoMotoContext.Users.Include(x => x.SearchLinks).FirstOrDefaultAsync(x =>
                x.TelegramChatId == update.CallbackQuery.Message.Chat.Id);

            if (user == null)
                return;

            user.SearchLinks.RemoveAll(x => x.Id == guidLinkId);

            await _otoMotoContext.SaveChangesAsync();

            await _telegramBot.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Usunieto link");

            // Show default view
            await _defaultCommand.ExecuteAsync(update);
        }
    }
}
