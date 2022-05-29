using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Commands
{
    public class OptionsLinkCommand : BaseCommand
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly TelegramBotClient _telegramBot;

        public OptionsLinkCommand(OtoMotoContext otoMotoContext, TelegramBot telegramBot)
        {
            _otoMotoContext = otoMotoContext;
            _telegramBot = telegramBot.GetBot().Result;
        }

        public override string Name => CommandNames.OptionsLinkCommand;
        public override async Task ExecuteAsync(Update update)
        {
            await _telegramBot.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

            if (!Guid.TryParse(update.CallbackQuery.Data.Replace(CommandNames.OptionsLinkCommand, string.Empty), out Guid guidLinkId))
                return;

            var searchLink = await _otoMotoContext.SearchLinks.FirstOrDefaultAsync(x => x.Id == guidLinkId);

            if (searchLink == null)
                return;

            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                    new []
                    {
                        new InlineKeyboardButton("Cofnij") {CallbackData = CommandNames.DefaultCommand},
                        new InlineKeyboardButton("Usun link") {CallbackData = $"{CommandNames.RemoveLinkCommand}{guidLinkId}"}
                    }
                });

            await _telegramBot.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId, $"Wybierz akcje: {searchLink.Link}", replyMarkup: inlineKeyboard);
        }
    }
}
