using Coravel.Invocable;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler
{
    public class Worker : IInvocable
    {
        private readonly ILogger<Worker> _logger;
        private readonly OtoMotoContext _dbOtoMotoContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public Worker(ILogger<Worker> logger, OtoMotoContext dbOtoMotoContext, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _dbOtoMotoContext = dbOtoMotoContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Invoke()
        {
            try
            {
                var searchLinks = await _dbOtoMotoContext.SearchLinks.Include(x => x.Users).ToListAsync();

                //Remove search links when there are no users
                var searchLinksToRemove = searchLinks.Where(x => x.Users.Count == 0).ToList();
                if (searchLinksToRemove.Any())
                {
                    _dbOtoMotoContext.RemoveRange(searchLinksToRemove);
                    searchLinks.RemoveAll(x => x.Users.Count == 0);
                }

                foreach (var searchLink in searchLinks)
                {
                    var searchLinkDto = new SearchLink()
                    {
                        Id = searchLink.Id,
                        AdLinks = searchLink.AdLinks,
                        CreatedDateTime = searchLink.CreatedDateTime,
                        LastUpdateDateTime = searchLink.LastUpdateDateTime,
                        Link = searchLink.Link,
                        SearchCount = searchLink.SearchCount,
                        Users = searchLink.Users.Select(x => new User()
                        {
                            CreatedDateTime = x.CreatedDateTime,
                            Id = x.Id,
                            LastUpdateDateTime = x.LastUpdateDateTime,
                            TelegramChatId = x.TelegramChatId,
                            TelegramName = x.TelegramName
                        }).ToList()
                    };

                    await _publishEndpoint.Publish<SearchLink>(searchLinkDto);

                    searchLink.SearchCount++;

                    _logger.LogInformation($"{DateTime.Now} - Dodano link wyszukiwania do kolejki: {searchLink.Link}");
                }

                await _dbOtoMotoContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}");
            }
        }
    }
}
