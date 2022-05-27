using Redis.OM;
using Redis.OM.Searching;
using Shared.Models;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Commands;

namespace Telegram.Services
{
    public interface ICommandExecutorServices
    {
        Task Execute(Update update);
    }

    public class CommandExecutorService : ICommandExecutorServices
    {
        private readonly List<BaseCommand> _commands;
        private readonly IRedisCollection<TelegramCurrentAction> _currentRedisActions;
        private BaseCommand _lastBaseCommand;

        public CommandExecutorService(IServiceProvider serviceProvider, RedisConnectionProvider redis)
        {
            _commands = serviceProvider.GetServices<BaseCommand>().ToList();
            _currentRedisActions = redis.RedisCollection<TelegramCurrentAction>();
        }

        public async Task Execute(Update update)
        {
            if (update.Type == UpdateType.Message)
            {
                var currentRedisActions =
                    await _currentRedisActions.FindByIdAsync(update.Message.Chat.Id.ToString());

                if (currentRedisActions == null || currentRedisActions.CurrentAction == CommandNames.DefaultCommand || update.Message?.Text == "Cofnij")
                {
                    switch (update.Message?.Text)
                    {
                        case "Pokaż moje linki":
                            await ExecuteAsync(CommandNames.ShowMyLinksCommand, update);
                            break;
                        case "Dodaj link":
                            await ExecuteAsync(CommandNames.AddLinkCommand, update);
                            break;
                        default:
                            await ExecuteAsync(CommandNames.DefaultCommand, update);
                            break;
                    }
                }
                else if (currentRedisActions.CurrentAction == CommandNames.AddLinkCommand)
                {
                    await ExecuteAsync(CommandNames.AddLinkCommand, update);
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
