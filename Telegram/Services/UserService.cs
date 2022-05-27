using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot.Types;
using User = Shared.Entities.User;

namespace Telegram.Services
{
    public interface IUserService
    {
        Task<User> GetOrCreated(Update update);
    }

    public class UserService : IUserService
    {
        private readonly OtoMotoContext _otoMotoContext;

        public UserService(OtoMotoContext otoMotoContext)
        {
            _otoMotoContext = otoMotoContext;
        }

        public async Task<User> GetOrCreated(Update update)
        {
            var newUser = new User
            {
                TelegramChatId = update?.Message?.Chat.Id,
                TelegramName = update?.Message?.Chat.Username
            };

            var user = await _otoMotoContext.Users
                .Include(x => x.SearchLinks)
                .FirstOrDefaultAsync(x => x.TelegramChatId == newUser.TelegramChatId);

            if (user != null)
                return user;

            var result = await _otoMotoContext.Users.AddAsync(newUser);
            await _otoMotoContext.SaveChangesAsync();

            return result.Entity;
        }
    }
}
