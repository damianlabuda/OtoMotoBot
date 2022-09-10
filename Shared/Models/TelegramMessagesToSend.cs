using Shared.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace Shared.Models;

public class TelegramMessagesToSend
{
    public string Message { get; set; }
    public List<InlineKeyboardButton> InlineKeyboard { get; set; }
    public List<User> Users { get; set; }
}