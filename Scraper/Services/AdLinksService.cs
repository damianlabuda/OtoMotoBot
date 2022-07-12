using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models;

namespace Scraper.Services;

public class AdLinksService : IAdLinksService
{
    private readonly OtoMotoContext _otoMotoContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public AdLinksService(OtoMotoContext otoMotoContext, IPublishEndpoint publishEndpoint)
    {
        _otoMotoContext = otoMotoContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task GetAdLinksCount(SearchLink searchLink, User user)
    {
        var searchLinkFromDb = await _otoMotoContext.SearchLinks.Include(x => x.Users).AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == searchLink.Id);

        if (searchLinkFromDb == null)
            return;

        if (searchLinkFromDb.AdLinksCount > 0)
        {
            var telegramMessagesToSend = new TelegramMessagesToSend()
            {
                Message =
                    $"Zapisano {searchLinkFromDb.AdLinksCount} ogłoszeń dla linku:\r\n{searchLinkFromDb.Link}, " +
                    $"od tej pory będziesz otrzymywać powiadomoenia o nowo dodanych ogłoszeniach lub zmian cen już istniejących",
                Users = new List<User>(){user}
            };
            
            await _publishEndpoint.Publish<TelegramMessagesToSend>(telegramMessagesToSend);
            return;
        }

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

        await _publishEndpoint.Publish<SearchLink>(searchLinkDto,
            context => context.TimeToLive = TimeSpan.FromMinutes(5));

        for (int i = 0; i < 300; i++)
        {
            await Task.Delay(1000);

            var searchLinkDb = await _otoMotoContext.SearchLinks.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == searchLink.Id);

            if (searchLinkDb != null && searchLinkDb.AdLinksCount > 0)
            {
                var telegramMessagesToSend = new TelegramMessagesToSend()
                {
                    Message =
                        $"Zapisano {searchLinkDb.AdLinksCount} ogłoszeń dla linku:\r\n{searchLinkDb.Link}, " +
                        $"od tej pory będziesz otrzymywać powiadomoenia o nowo dodanych ogłoszeniach lub zmian cen już istniejących",
                    Users = new List<User>(){user}
                };
            
                await _publishEndpoint.Publish<TelegramMessagesToSend>(telegramMessagesToSend);
                break;
            }
        }
    }
}