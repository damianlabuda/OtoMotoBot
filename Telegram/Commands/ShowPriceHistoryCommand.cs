using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Commands;

public class ShowPriceHistoryCommand : IBaseCommand
{
    private readonly OtoMotoContext _otoMotoContext;
    private readonly ITelegramBotClient _telegramBotClient;

    public ShowPriceHistoryCommand(OtoMotoContext otoMotoContext, ITelegramBotClient telegramBotClient)
    {
        _otoMotoContext = otoMotoContext;
        _telegramBotClient = telegramBotClient;
    }
    
    public string Name => CommandNames.ShowPriceHistoryCommand;
    public async Task ExecuteAsync(Update update)
    {
        await _telegramBotClient.SendChatActionAsync(update.CallbackQuery.Message.Chat.Id, ChatAction.Typing);

        if (!long.TryParse(update.CallbackQuery.Data.Replace(CommandNames.ShowPriceHistoryCommand, String.Empty), out long longAdLinkId) || !(longAdLinkId > 0))
            return;

        var adLink = await _otoMotoContext.AdLinks.Include(x => x.Prices).FirstOrDefaultAsync(x => x.Id == longAdLinkId);

        if (adLink == null)
            return;
        
        if (adLink.Prices.Count <= 1)
            return;
        
        string text = update.CallbackQuery.Message.Text + "\n\n" + string.Join("\n",
            adLink.Prices.OrderByDescending(s => s.CreatedDateTime).Take(10).OrderBy(s => s.CreatedDateTime).Select(x =>
                $"{x.CreatedDateTime.AddHours(2).ToString("dd.MM HH:mm")} - {x.Price} {x.Currency}"));
        
        await _telegramBotClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat.Id,
            update.CallbackQuery.Message.MessageId, text);
    }
}