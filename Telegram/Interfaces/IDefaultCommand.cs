using Telegram.Bot.Types;

namespace Telegram.Interfaces;

public interface IDefaultCommand
{
    Task ExecuteAsync(Update update);
}