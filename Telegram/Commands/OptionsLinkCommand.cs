using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Commands
{
    public class OptionsLinkCommand : IBaseCommand
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly ITelegramBotClient _telegramBotClient;

        public OptionsLinkCommand(OtoMotoContext otoMotoContext, ITelegramBotClient telegramBotClient)
        {
            _otoMotoContext = otoMotoContext;
            _telegramBotClient = telegramBotClient;
        }

        public string Name => CommandNames.OptionsLinkCommand;
        public async Task ExecuteAsync(Update update)
        {
            await _telegramBotClient.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

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

            await _telegramBotClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId, $"Wybierz akcje: {searchLink.Link}", replyMarkup: inlineKeyboard);
        }
    }
}
