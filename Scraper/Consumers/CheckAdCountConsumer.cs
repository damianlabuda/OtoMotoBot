using MassTransit;
using Scraper.Interfaces;
using Shared.Models;

namespace Scraper.Consumers;

public class CheckAdCountConsumer : IConsumer<TelegramCheckAdCount>
{
    private readonly ILogger<CheckAdCountConsumer> _logger;
    private readonly IAdLinksService _adLinksService;

    public CheckAdCountConsumer(ILogger<CheckAdCountConsumer> logger, IAdLinksService adLinksService)
    {
        _logger = logger;
        _adLinksService = adLinksService;
    }
    
    public async Task Consume(ConsumeContext<TelegramCheckAdCount> context)
    {
        try
        {
            var telegramCheckAdCount = context.Message;

            await _adLinksService.GetAdLinksCount(telegramCheckAdCount.SearchLink, telegramCheckAdCount.User);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }
    }
}