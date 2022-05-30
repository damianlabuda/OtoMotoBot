using Telegram.Bot.Types;

namespace Telegram.Interfaces;

public interface ICommandExecutorService
{
    Task Execute(Update update);
}