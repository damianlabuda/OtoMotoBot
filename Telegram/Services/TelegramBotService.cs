using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Telegram.Services
{
    public interface ITelegramBotService
    {
        Task Update(object update);
    }

    public class TelegramBotService : ITelegramBotService
    {
        private readonly ICommandExecutorServices _commandExecutorServices;


        public TelegramBotService(ICommandExecutorServices commandExecutorServices)
        {
            _commandExecutorServices = commandExecutorServices;
        }

        public async Task Update(object update)
        {
            var deserializedUpdate = JsonConvert.DeserializeObject<Update>(update.ToString());

            if (deserializedUpdate?.Message?.Chat == null)
                return;

            await _commandExecutorServices.Execute(deserializedUpdate);
        }
    }
}
