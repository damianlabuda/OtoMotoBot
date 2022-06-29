using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Sender.Interfaces;
using Shared.Entities;
using Shared.Models;
using Telegram.Bot;

namespace Sender.Services
{
    public class TelegramSenderService : ITelegramSenderService
    {
        private readonly ILogger<TelegramSenderService> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly OtoMotoContext _otoMotoContext;
        private readonly SemaphoreSlim _semaphoreSlim = new(10, 10);
        private readonly object _obj = new();
        private readonly List<Guid> _usersTelegramChatNotFound = new();
        private int MessagesSendCounter { get; set; }
        private int MessagesErrorCounter { get; set; }
        
        public TelegramSenderService(ILogger<TelegramSenderService> logger, ITelegramBotClient telegramBotClient,
            OtoMotoContext otoMotoContext)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _otoMotoContext = otoMotoContext;
        }

        public async Task SendsAsync(List<NewAdMessage> newAdMessages)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var task = newAdMessages.Select(SendMessagesAsync);

            await Task.WhenAll(task);

            await SetInfoAboutUserValidityTelegramChat();

            stopwatch.Stop();

            _logger.LogInformation($"{DateTime.Now} - Wysłano: {MessagesSendCounter} nowych wiadomości," +
                                    $" z {MessagesSendCounter + MessagesErrorCounter} zaplanowanych," +
                                   $" czas {stopwatch.Elapsed}");
        }
        
        private async Task SendMessagesAsync(NewAdMessage newAdMessage)
        {
            await _semaphoreSlim.WaitAsync();

            var users = newAdMessage.Users;

            try
            {
                foreach (var user in users)
                {
                    try
                    {
                        string text = newAdMessage.PriceBefore == 0
                            ? $"Nowe ogłoszenie, cena: {newAdMessage.Price}\nhttps://www.otomoto.pl/{newAdMessage.Id}"
                            : $"Zmiana ceny z {newAdMessage.PriceBefore}, na {newAdMessage.Price}\nhttps://www.otomoto.pl/{newAdMessage.Id}";

                        await _telegramBotClient.SendTextMessageAsync(user.TelegramChatId, text);

                        lock (_obj) 
                            MessagesSendCounter++;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Error message: {e.Message}, User Id: {user.Id}");

                        lock (_obj)
                        {
                            MessagesErrorCounter++;

                            if (e.Message == "Bad Request: chat not found" || e.Message == "Forbidden: bot was blocked by the user")
                            {
                                _usersTelegramChatNotFound.Add(user.Id);
                            }
                        }
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        
        private async Task SetInfoAboutUserValidityTelegramChat()
        {
            if (_usersTelegramChatNotFound.Any())
            {
                var users = await _otoMotoContext.Users.Where(x => _usersTelegramChatNotFound.Distinct().Contains(x.Id))
                    .ToListAsync();
                users.ForEach(x => x.TelegramChatNotFound = true);
                await _otoMotoContext.SaveChangesAsync();
            }
        }
    }
}