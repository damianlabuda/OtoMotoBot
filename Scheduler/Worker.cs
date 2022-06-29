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
                // Get all searchLinks from db
                var searchLinks =
                    await _dbOtoMotoContext.SearchLinks
                        .Include(x => x.Users.Where(u => u.TelegramChatNotFound == false)).ToListAsync();
                
                // Remove searchLinks from db when there are no users
                var searchLinksToRemove = searchLinks.Where(x => x.Users.Count == 0).ToList();
                if (searchLinksToRemove.Any())
                {
                    _dbOtoMotoContext.SearchLinks.RemoveRange(searchLinksToRemove);
                    await _dbOtoMotoContext.SaveChangesAsync();
                    
                    // Remove searchLinks from list when there are no users
                    searchLinks.RemoveAll(x => x.Users.Count == 0);
                }
                
                // Remove adLinks that don't have any searchLinks
                var adLinksToRemove = await _dbOtoMotoContext.AdLinks.Where(x => x.SearchLinks.Count == 0).ToListAsync();
                if (adLinksToRemove.Any())
                {
                    _dbOtoMotoContext.AdLinks.RemoveRange(adLinksToRemove);
                    await _dbOtoMotoContext.SaveChangesAsync();
                }
                
                // Add all searchLinks to RabbitMq queue
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

                    await _publishEndpoint.Publish<SearchLink>(searchLinkDto, context => context.TimeToLive = TimeSpan.FromMinutes(5));

                    _logger.LogInformation($"{DateTime.Now} - Dodano link wyszukiwania do kolejki: {searchLink.Link}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}");
            }
        }
    }
}
