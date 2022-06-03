using MassTransit;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models;

namespace Scraper.Services
{
    public class SearchLinkQueueConsumerService : IConsumer<SearchLink>
    {
        private readonly ILogger<SearchLinkQueueConsumerService> _logger;
        private readonly ISearchAuctionsService _searchAuctionsService;
        private readonly ICheckInDbService _checkInDbService;
        private readonly IPublishEndpoint _publishEndpoint;

        public SearchLinkQueueConsumerService(ILogger<SearchLinkQueueConsumerService> logger,
            ISearchAuctionsService searchAuctionsService, ICheckInDbService checkInDbService,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _searchAuctionsService = searchAuctionsService;
            _checkInDbService = checkInDbService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<SearchLink> context)
        {
            try
            {
                var searchLink = context.Message;

                var adLinks = await _searchAuctionsService.Search(searchLink);

                if (adLinks.Any())
                {
                    var newAdMessages = await _checkInDbService.Check(adLinks);

                    if (newAdMessages.Any() && searchLink.SearchCount > 0)
                    {
                        MessagesToSent messagesToSent = new MessagesToSent()
                        {
                            NewAdMessages = newAdMessages,
                            Users = searchLink.Users
                        };

                        await _publishEndpoint.Publish<MessagesToSent>(messagesToSent);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }
}
