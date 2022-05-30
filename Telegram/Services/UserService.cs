using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Interfaces;
using User = Shared.Entities.User;

namespace Telegram.Services
{ 
    public class UserService : IUserService
    {
        private readonly OtoMotoContext _otoMotoContext;

        public UserService(OtoMotoContext otoMotoContext)
        {
            _otoMotoContext = otoMotoContext;
        }

        public async Task<User> GetOrCreated(Update update)
        {
            var newUser = new User();

            if (update.Type == UpdateType.Message)
            {
                newUser.TelegramChatId = update.Message?.Chat.Id;
                newUser.TelegramName = update.Message?.Chat.Username;
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                newUser.TelegramChatId = update.CallbackQuery?.Message?.Chat.Id;
                newUser.TelegramName = update.CallbackQuery?.Message?.Chat.Username;
            }

            var user = await _otoMotoContext.Users
                .Include(x => x.SearchLinks)
                .FirstOrDefaultAsync(x => x.TelegramChatId == newUser.TelegramChatId);

            if (user != null)
                return user;

            var result = await _otoMotoContext.Users.AddAsync(newUser);
            await _otoMotoContext.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<User> Get(Update update)
        {
            long telegramChatId = 0;

            if (update.Type == UpdateType.Message)
                telegramChatId = (long)update.Message?.Chat.Id;

            if (update.Type == UpdateType.CallbackQuery)
                telegramChatId = (long)update.CallbackQuery?.Message?.Chat.Id;

            var user = await _otoMotoContext.Users
                .Include(x => x.SearchLinks)
                .FirstOrDefaultAsync(x => x.TelegramChatId == telegramChatId);

            return user;
        }
    }
}
