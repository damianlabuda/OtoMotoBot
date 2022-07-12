using MassTransit;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models;

namespace Scraper.Consumers
{
    public class CheckSearchLinksConsumer : IConsumer<SearchLink>
    {
        private readonly ILogger<CheckSearchLinksConsumer> _logger;
        private readonly ISearchAuctionsService _searchAuctionsService;
        private readonly ICheckInDbService _checkInDbService;
        private readonly IPublishEndpoint _publishEndpoint;

        public CheckSearchLinksConsumer(ILogger<CheckSearchLinksConsumer> logger,
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
                    var newAdMessages = await _checkInDbService.Check(adLinks, searchLink);

                    if (newAdMessages.Any() && searchLink.SearchCount > 0)
                    {
                        foreach (var newAdMessage in newAdMessages)
                        {
                            string text = newAdMessage.PriceBefore == 0
                                ? $"Nowe ogłoszenie, cena: {newAdMessage.Price} {newAdMessage.Currency}\nhttps://www.otomoto.pl/{newAdMessage.Id}"
                                : $"Zmiana ceny z {newAdMessage.PriceBefore} {newAdMessage.CurrencyBefore}, na {newAdMessage.Price} {newAdMessage.Currency}\nhttps://www.otomoto.pl/{newAdMessage.Id}";

                            var telegramMessagesToSend = new TelegramMessagesToSend()
                            {
                                Message = text,
                                Users = newAdMessage.Users
                            };

                            await _publishEndpoint.Publish<TelegramMessagesToSend>(telegramMessagesToSend);
                        }
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
