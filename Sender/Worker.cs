using MassTransit;
using Sender.Interfaces;
using Shared.Models;

namespace Sender
{
    public class Worker : IConsumer<MessagesToSent>
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITelegramSenderService _telegramSenderService;

        public Worker(ILogger<Worker> logger, ITelegramSenderService telegramSenderService)
        {
            _logger = logger;
            _telegramSenderService = telegramSenderService;
        }

        public async Task Consume(ConsumeContext<MessagesToSent> context)
        {
            try
            {
                var messagesToSent = context.Message;

                await _telegramSenderService.SendsAsync(messagesToSent.Users, messagesToSent.NewAdMessages);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
