using Shared.Entities;
using Shared.Models;

namespace Sender.Interfaces;

public interface ITelegramSenderService
{
    Task SendsAsync(TelegramMessagesToSend telegramMessagesToSend);
}