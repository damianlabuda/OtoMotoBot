using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Commands
{
    public class RemoveLinkCommand : IBaseCommand
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IDefaultCommand _defaultCommand;

        public RemoveLinkCommand(OtoMotoContext otoMotoContext, ITelegramBotClient telegramBotClient, IDefaultCommand defaultCommand)
        {
            _otoMotoContext = otoMotoContext;
            _telegramBotClient = telegramBotClient;
            _defaultCommand = defaultCommand;
        }
        public string Name => CommandNames.RemoveLinkCommand;
        public async Task ExecuteAsync(Update update)
        {
            await _telegramBotClient.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

            if (!Guid.TryParse(update.CallbackQuery.Data.Replace(CommandNames.RemoveLinkCommand, string.Empty), out Guid guidLinkId))
                return;

            var user = await _otoMotoContext.Users.Include(x => x.SearchLinks).FirstOrDefaultAsync(x =>
                x.TelegramChatId == update.CallbackQuery.Message.Chat.Id);

            if (user == null)
                return;

            user.SearchLinks.RemoveAll(x => x.Id == guidLinkId);

            await _otoMotoContext.SaveChangesAsync();

            await _telegramBotClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Usunieto link");

            // Show default view
            await _defaultCommand.ExecuteAsync(update);
        }
    }
}
