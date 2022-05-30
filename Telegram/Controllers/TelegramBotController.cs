using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
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
            await _commandExecutorService.Execute(update);

            return Ok();
        }
    }
}
