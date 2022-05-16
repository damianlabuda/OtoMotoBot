using Shared.Models;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = Shared.Entities.User;

namespace Sender
{
    public class TelegramSender
    {
        private readonly List<User> _users;

        private readonly List<NewAdMessage> _newAdMessages;

        private readonly IServiceScopeFactory _iServiceScopeFactory;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(10, 10);

        private int MessagesSendCounter { get; set; } = 0;

        private readonly object _obj = new object();
        
        public TelegramSender(List<User> users, List<NewAdMessage> newAdMessages, IServiceScopeFactory iServiceScopeFactory)
        {
            _users = users;
            _newAdMessages = newAdMessages;
            _iServiceScopeFactory = iServiceScopeFactory;
        }

        public async Task<string> SendsAsync()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var task = _users.Select(SendMessagesAsync);

            await Task.WhenAll(task);

            stopwatch.Stop();

            return $"{DateTime.Now} - Wysłano: {MessagesSendCounter} nowych wiadomości," +
                   $" z {_newAdMessages.Count * _users.Count} zaplanowanych," +
                   $" dla {_users.Count} użytkowników," +
                   $" czas {stopwatch.Elapsed}";
        }

        private async Task SendMessagesAsync(User user)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                if (user.TelegramChatId != null)
                {
                    using var scope = _iServiceScopeFactory.CreateScope();
                    var telegramClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

                    foreach (NewAdMessage newAdMessage in _newAdMessages)
                    {
                        string text = newAdMessage.PriceBefore == 0 
                            ? $"Nowe ogłoszenie\n{newAdMessage.Link}" 
                            : $"Zmiana ceny z {newAdMessage.PriceBefore}, na {newAdMessage.Price}\n{newAdMessage.Link}";

                        await telegramClient.SendTextMessageAsync(new ChatId((long)user.TelegramChatId), text);

                        lock (_obj)
                            MessagesSendCounter++;
                    }
                }
            }
            catch (Exception) { }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
} 