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
        private IBaseCommand _lastBaseCommand;

        public CommandExecutorService(IServiceProvider serviceProvider, RedisConnectionProvider redis)
        {
            _commands = serviceProvider.GetServices<IBaseCommand>().ToList();
            _currentRedisActions = redis.RedisCollection<TelegramCurrentActionRedis>();
        }

        public async Task Execute(Update update)
        {
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
            }
        }

        private async Task ExecuteAsync(string commandName, Update update)
        {
            _lastBaseCommand = _commands.First(x => x.Name == commandName);
            await _lastBaseCommand.ExecuteAsync(update);
        }
    }
}
