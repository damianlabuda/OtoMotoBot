using Telegram.Bot.Types;

namespace Telegram.Interfaces;

public interface IBaseCommand
{
    string Name { get; }
    public Task ExecuteAsync(Update update);
}