using MassTransit;
using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Redis.OM.Searching;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;
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
        private readonly ILogger<AddLinkCommand> _logger;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly OtoMotoContext _otoMotoContext;
        private readonly IUserService _userService;
        private readonly RedisConnectionProvider _redis;
        private readonly ISearchLinkService _searchLinkService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRedisCollection<TelegramCurrentActionRedis> _currentRedisActions;
        private string _link;
        private long _chatId;

        private readonly InlineKeyboardMarkup _inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                new InlineKeyboardButton("Cofnij") {CallbackData = CommandNames.DefaultCommand}
            }
        });

        public AddLinkCommand(ILogger<AddLinkCommand> logger, ITelegramBotClient telegramBotClient, OtoMotoContext otoMotoContext,
            IUserService userService, RedisConnectionProvider redis, ISearchLinkService searchLinkService,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _telegramBotClient = telegramBotClient;
            _otoMotoContext = otoMotoContext;
            _userService = userService;
            _redis = redis;
            _searchLinkService = searchLinkService;
            _publishEndpoint = publishEndpoint;
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
                var telegramCheckAdCount = await AddSearchLinkToDb(update);
                if (telegramCheckAdCount == null)
                    return;

                // Publish task in queue for send to user information about how many ad count search link 
                try
                {
                    await _publishEndpoint.Publish<TelegramCheckAdCount>(telegramCheckAdCount);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                // Set redis value
                await _redis.Connection.UnlinkAsync($"{typeof(TelegramCurrentActionRedis)}:{_chatId}");

                // Send info to user
                await _telegramBotClient.SendTextMessageAsync(_chatId, $"Dodano link:\r\n{_link}",
                    replyMarkup: _inlineKeyboard, disableNotification: false);
            }
        }

        private async Task<TelegramCheckAdCount> AddSearchLinkToDb(Update update)
        {
            var user = await _userService.GetOrCreated(update);

            if (user.SearchLinks != null)
            {
                if (user.SearchLinks.Count >= 5)
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, "Możesz posiadać max 5 linków wyszukiwania",
                        replyMarkup: _inlineKeyboard);
                    return null;
                    //return false;
                }

                if (user.SearchLinks.Any(x => x.Link == _link))
                {
                    await _telegramBotClient.SendTextMessageAsync(_chatId, "Link wyszukiwania jest już dodany",
                        replyMarkup: _inlineKeyboard);
                    // return false;
                    return null;
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
                    Users = new List<User>() {user}
                };

                searchLinkFromDb = (await _otoMotoContext.SearchLinks.AddAsync(searchLink)).Entity;
            }
            else
            {
                searchLinkFromDb.Users.Add(user);
            }

            await _otoMotoContext.SaveChangesAsync();
            // return true;
            
            var searchLinkDto = new SearchLink()
            {
                Id = searchLinkFromDb.Id,
                AdLinks = searchLinkFromDb.AdLinks,
                CreatedDateTime = searchLinkFromDb.CreatedDateTime,
                LastUpdateDateTime = searchLinkFromDb.LastUpdateDateTime,
                Link = searchLinkFromDb.Link,
                SearchCount = searchLinkFromDb.SearchCount,
                AdLinksCount = searchLinkFromDb.AdLinksCount,
                Users = searchLinkFromDb.Users.Select(x => new User()
                {
                    CreatedDateTime = x.CreatedDateTime,
                    Id = x.Id,
                    LastUpdateDateTime = x.LastUpdateDateTime,
                    TelegramChatId = x.TelegramChatId,
                    TelegramName = x.TelegramName
                }).ToList()
            };

            var userDto = new User()
            {
                Id = user.Id,
                CreatedDateTime = user.CreatedDateTime,
                LastUpdateDateTime = user.LastUpdateDateTime,
                TelegramChatId = user.TelegramChatId,
                TelegramChatNotFound = user.TelegramChatNotFound
            };
            
            return new TelegramCheckAdCount() { SearchLink = searchLinkDto, User = userDto};
        }
    }
}