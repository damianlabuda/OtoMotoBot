using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Redis.OM.Searching;
using Shared.Entities;
using Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Services;
using User = Shared.Entities.User;

namespace Telegram.Commands
{
    public class AddLinkCommand : BaseCommand
    {
        private readonly TelegramBotClient _telegramBot;
        private readonly OtoMotoContext _otoMotoContext;
        private readonly IUserService _userService;
        private readonly IRedisCollection<TelegramCurrentAction> _currentActions;
        private string _link;
        private long _chatId;
        private readonly ReplyKeyboardMarkup _keyboardButtonBack = new(new[]
        {
            new KeyboardButton[] { "Cofnij" },
        })
        {
            ResizeKeyboard = true
        };

        public AddLinkCommand(TelegramBot telegramBot, OtoMotoContext otoMotoContext,
            IUserService userService, RedisConnectionProvider redis)
        {
            _telegramBot = telegramBot.GetBot().Result;
            _otoMotoContext = otoMotoContext;
            _userService = userService;
            _currentActions = redis.RedisCollection<TelegramCurrentAction>();
        }

        public override string Name => CommandNames.AddLinkCommand;
        public override async Task ExecuteAsync(Update update)
        {
            _chatId = update.Message.Chat.Id;

            var currentActions = await _currentActions.FindByIdAsync(_chatId.ToString());

            if (currentActions == null || currentActions.CurrentAction != CommandNames.AddLinkCommand)
            {
                // Send text to chat
                string text =
                    "Wyślij swój link wyszukiwania, np." +
                    "\r\nhttps://www.otomoto.pl/osobowe/volkswagen/passat/od-2015?search%5Bfilter_enum_gearbox%5D=automatic";

                await _telegramBot.SendTextMessageAsync(_chatId, text, replyMarkup: _keyboardButtonBack, disableWebPagePreview: true);

                // Set redis current action value
                await _currentActions.InsertAsync(new TelegramCurrentAction()
                {
                    CurrentAction = CommandNames.AddLinkCommand,
                    TelegramChatId = _chatId
                });
            }
            else
            {
                // Send typing to chat
                await _telegramBot.SendChatActionAsync(_chatId, ChatAction.Typing);

                // Check link from message
                if (!CheckLink(update))
                {
                    await _telegramBot.SendTextMessageAsync(_chatId, "Wyślij poprawny link", replyMarkup: _keyboardButtonBack);
                    return;
                }

                // Add search link to db
                if (!(await AddSearchLinkToDb(update)))
                    return;

                // Set redis value
                currentActions.CurrentAction = CommandNames.DefaultCommand;
                await _currentActions.Update(currentActions);

                //Send finish message
                ReplyKeyboardMarkup keyboardButtonDefault = new(new[]
                {
                    new KeyboardButton[] { "Pokaż moje linki", "Dodaj link" },
                })
                {
                    ResizeKeyboard = true
                };

                await _telegramBot.SendTextMessageAsync(_chatId, $"Dodano link wyszukiwania:\r\n{_link}"
                    , replyMarkup: keyboardButtonDefault, disableWebPagePreview: true);
            }
        }

        private async Task<bool> AddSearchLinkToDb(Update update)
        {
            var user = await _userService.GetOrCreated(update);

            if (user.SearchLinks != null)
            {
                if (user.SearchLinks.Count >= 10)
                {
                    await _telegramBot.SendTextMessageAsync(_chatId, "Możesz posiadać max 10 linków wyszukiwania", replyMarkup: _keyboardButtonBack);
                    return false;
                }

                if (user.SearchLinks.Any(x => x.Link == _link))
                {
                    await _telegramBot.SendTextMessageAsync(_chatId, "Link wyszukiwania jest już dodany", replyMarkup: _keyboardButtonBack);
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

        private bool CheckLink(Update update)
        {
            string linkForCheck = update?.Message?.Text;

            if (string.IsNullOrEmpty(linkForCheck))
                return false;

            if (!Uri.IsWellFormedUriString(linkForCheck, UriKind.Absolute))
                return false;

            Uri baseUri = new Uri(linkForCheck);

            if (baseUri.Host != "www.otomoto.pl")
                return false;

            _link = linkForCheck;
            return true;
        }
    }
}
