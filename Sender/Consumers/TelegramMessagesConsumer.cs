﻿using MassTransit;
using Sender.Interfaces;
using Shared.Models;

namespace Sender.Consumers
{
    // public class Worker : IConsumer<MessagesToSent>
    public class TelegramMessagesConsumer : IConsumer<TelegramMessagesToSend>
    {
        private readonly ILogger<TelegramMessagesConsumer> _logger;
        private readonly ITelegramSenderService _telegramSenderService;

        public TelegramMessagesConsumer(ILogger<TelegramMessagesConsumer> logger, ITelegramSenderService telegramSenderService)
        {
            _logger = logger;
            _telegramSenderService = telegramSenderService;
        }

        // public async Task Consume(ConsumeContext<MessagesToSent> context)
        public async Task Consume(ConsumeContext<TelegramMessagesToSend> context)
        {
            try
            {
                // var messagesToSent = context.Message;
                var telegramMessagesToSend = context.Message;

                // await _telegramSenderService.SendsAsync(messagesToSent.NewAdMessages);
                await _telegramSenderService.SendsAsync(telegramMessagesToSend);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
