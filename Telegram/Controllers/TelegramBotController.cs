using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Interfaces;

namespace Telegram.Controllers
{
    public class TelegramBotController : ControllerBase
    {
        private readonly ICommandExecutorService _commandExecutorService;

        public TelegramBotController(ICommandExecutorService commandExecutorService)
        {
            _commandExecutorService = commandExecutorService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update.Type == UpdateType.Message || update.Type == UpdateType.CallbackQuery)
                await _commandExecutorService.Execute(update);

            return Ok();
        }
    }
}
