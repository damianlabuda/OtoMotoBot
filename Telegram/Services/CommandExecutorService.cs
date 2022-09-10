using Redis.OM;
using Redis.OM.Searching;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Interfaces;
using Telegram.Models;

namespace Telegram.Services
{
    public class CommandExecutorService : ICommandExecutorService
    {
        private readonly List<IBaseCommand> _commands;
        private readonly IRedisCollection<TelegramCurrentActionRedis> _currentRedisActions;
        private readonly IRedisCollection<TelegramTimeLastActionRedis> _timeOfLastActionRedis;
        private IBaseCommand _lastBaseCommand;

        public CommandExecutorService(IServiceProvider serviceProvider, RedisConnectionProvider redis)
        {
            _commands = serviceProvider.GetServices<IBaseCommand>().ToList();
            _currentRedisActions = redis.RedisCollection<TelegramCurrentActionRedis>();
            _timeOfLastActionRedis = redis.RedisCollection<TelegramTimeLastActionRedis>();
        }

        public async Task Execute(Update update)
        {
            if (!await CheckCoolDownAction(update))
                return;

            if (update.Type == UpdateType.Message)
            {
                var currentRedisActions =
                    await _currentRedisActions.FindByIdAsync(update.Message.Chat.Id.ToString());

                if (currentRedisActions == null)
                {
                    await ExecuteAsync(CommandNames.DefaultCommand, update);
                    return;
                }
                if (currentRedisActions.CurrentAction == CommandNames.AddLinkCommand)
                {
                    await ExecuteAsync(CommandNames.AddLinkCommand, update);
                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                switch (update.CallbackQuery?.Data)
                {
                    case CommandNames.ShowMyLinksCommand:
                        await ExecuteAsync(CommandNames.ShowMyLinksCommand, update);
                        return;
                    case CommandNames.AddLinkCommand:
                        await ExecuteAsync(CommandNames.AddLinkCommand, update);
                        return;
                    case CommandNames.DefaultCommand:
                        await ExecuteAsync(CommandNames.DefaultCommand, update);
                        return;
                }

                if (update.CallbackQuery.Data.Contains(CommandNames.OptionsLinkCommand))
                {
                    await ExecuteAsync(CommandNames.OptionsLinkCommand, update);
                    return;
                }

                if (update.CallbackQuery.Data.Contains(CommandNames.RemoveLinkCommand))
                {
                    await ExecuteAsync(CommandNames.RemoveLinkCommand, update);
                    return;
                }

                if (update.CallbackQuery.Data.Contains(CommandNames.ShowPriceHistoryCommand))
                {
                    await ExecuteAsync(CommandNames.ShowPriceHistoryCommand, update);
                    return;
                }
            }
        }

        private async Task ExecuteAsync(string commandName, Update update)
        {
            _lastBaseCommand = _commands.First(x => x.Name == commandName);
            await _lastBaseCommand.ExecuteAsync(update);
        }

        private async Task<bool> CheckCoolDownAction(Update update)
        {
            long chatId = 0;
            double coolDown = 1;

            if (update.Type == UpdateType.Message)
                chatId = update.Message.Chat.Id;
            if (update.Type == UpdateType.CallbackQuery)
                chatId = update.CallbackQuery.Message.Chat.Id;

            var timeOfLastAction = await _timeOfLastActionRedis.FindByIdAsync(chatId.ToString());

            await _timeOfLastActionRedis.InsertAsync(new TelegramTimeLastActionRedis()
            {
                TelegramChatId = chatId,
                Time = DateTime.UtcNow.AddSeconds(coolDown)
            });

            if (timeOfLastAction == null || timeOfLastAction.Time < DateTime.UtcNow)
                return true;

            return false;
        }
    }
}
