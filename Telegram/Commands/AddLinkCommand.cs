using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Redis.OM.Searching;
using Shared.Entities;
using Shared.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Interfaces;
using Telegram.Models;
using User = Shared.Entities.User;

namespace Telegram.Commands
{
    public class AddLinkCommand : IBaseCommand
    {
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly OtoMotoContext _otoMotoContext;
        private readonly IUserService _userService;
        private readonly RedisConnectionProvider _redis;
        private readonly ISearchLinkService _searchLinkService;
        private readonly IRedisCollection<TelegramCurrentActionRedis> _currentRedisActions;
        private string _link;
        private long _chatId;
        private readonly InlineKeyboardMarkup _inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new []
            {
                new InlineKeyboardButton("Cofnij") { CallbackData = CommandNames.DefaultCommand }
            }
        });

        public AddLinkCommand(ITelegramBotClient telegramBotClient, OtoMotoContext otoMotoContext,
            IUserService userService, RedisConnectionProvider redis, IDefaultCommand defaultCommand, ISearchLinkService searchLinkService)
        {
            _telegramBotClient = telegramBotClient;
            _otoMotoContext = otoMotoContext;
            _userService = userService;
            _redis = redis;
            _searchLinkService = searchLinkService;
            _currentRedisActions = redis.RedisCollection<TelegramCurrentActionRedis>();
        }

        public string Name => CommandNames.AddLinkCommand;
        public async Task ExecuteAsync(Update update)
        {
            if (update.Type == UpdateType.CallbackQuery)
            {
                _chatId = update.CallbackQuery.Message.Chat.Id;

                await _telegramBotClient.SendChatActionAsync(_chatId, ChatAction.Typing);

                // Send text to chat
                string text =
                    "Wyślij swój link wyszukiwania, np." +
                    "\r\nhttps://www.otomoto.pl/osobowe/volkswagen/passat/od-2015?search%5Bfilter_enum_gearbox%5D=automatic";

                await _telegramBotClient.EditMessageTextAsync(_chatId,
                    update.CallbackQuery.Message.MessageId, text, replyMarkup: _inlineKeyboard);

                // Set redis current action value
                await _currentRedisActions.InsertAsync(new TelegramCurrentActionRedis()
                {
                    CurrentAction = CommandNames.AddLinkCommand,
                    TelegramChatId = _chatId
                });

                return;
            }

            if (update.Type == UpdateType.Message)
            {
                _chatId = update.Message.Chat.Id;
                _link = update.Message?.Text?.Trim();

                // Send typing to chat
                await _telegramBotClient.SendChatActionAsync(_chatId, ChatAction.Typing);

                // Check link from message
                if (string.IsNullOrEmpty(_searchLinkService.GenerateLinkToApi(_link, 0, String.Empty)))
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, "Wyślij poprawny link",
                        replyMarkup: _inlineKeyboard);
                    return;
                }

                // Add search link to db
                if (!(await AddSearchLinkToDb(update)))
                    return;

                // Set redis value
                await _redis.Connection.UnlinkAsync($"{typeof(TelegramCurrentActionRedis)}:{_chatId}");

                await _telegramBotClient.SendTextMessageAsync(_chatId, $"Dodano link:\r\n{_link}",
                    replyMarkup: _inlineKeyboard, disableNotification: false);
            }
        }

        private async Task<bool> AddSearchLinkToDb(Update update)
        {
            var user = await _userService.GetOrCreated(update);

            if (user.SearchLinks != null)
            {
                if (user.SearchLinks.Count >= 5)
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, "Możesz posiadać max 5 linków wyszukiwania",
                        replyMarkup: _inlineKeyboard);
                    return false;
                }

                if (user.SearchLinks.Any(x => x.Link == _link))
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, "Link wyszukiwania jest już dodany",
                        replyMarkup: _inlineKeyboard);
                    return false;
                }
            }

            var searchLinkFromDb = await _otoMotoContext.SearchLinks
                .Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Link == _link);

            if (searchLinkFromDb == null)
            {
                SearchLink searchLink = new SearchLink()
                {
                    Link = _link,
                    Users = new List<User>() { user }
                };

                await _otoMotoContext.SearchLinks.AddAsync(searchLink);
            }
            else
            {
                searchLinkFromDb.Users.Add(user);
            }

            await _otoMotoContext.SaveChangesAsync();
            return true;
        }
    }
}
