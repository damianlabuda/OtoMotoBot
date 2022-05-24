using Microsoft.AspNetCore.Mvc;
using Telegram.Services;

namespace Telegram.Controllers
{
    [ApiController]
    [Route("api/telegram/update")]
    public class TelegramBotController : ControllerBase
    {
        private readonly ITelegramBotService _telegramBotService;

        public TelegramBotController(ITelegramBotService telegramBotService)
        {
            _telegramBotService = telegramBotService;
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] object update)
        {
            await _telegramBotService.Update(update);

            return Ok();
        }
    }
}
