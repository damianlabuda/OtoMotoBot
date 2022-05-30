using Telegram.Bot.Types;
using User = Shared.Entities.User;

namespace Telegram.Interfaces;

public interface IUserService
{
    Task<User> GetOrCreated(Update update);
    Task<User> Get(Update update);
}