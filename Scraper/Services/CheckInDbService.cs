using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models;

namespace Scraper.Services
{
    public class CheckInDbService : ICheckInDbService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CheckInDbService> _logger;
        private readonly SemaphoreSlim _semaphoreSlim = new(10, 10);
        private readonly List<NewAdMessage> _newAdMessages = new();
        private readonly object _locker = new();
        private int NewRecords { get; set; } = 0;
        private int UpdatedPriceRecords { get; set; } = 0;

        public CheckInDbService(IServiceScopeFactory serviceScopeFactory, ILogger<CheckInDbService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        public async Task<List<NewAdMessage>> Check(List<AdLink> adLinks)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var tasks = adLinks.Select(CheckExistenceOrPriceInDb);

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            _logger.LogInformation($"{DateTime.Now} - {NewRecords} nowych rekordów, {UpdatedPriceRecords} zmian cen, czas: {stopwatch.Elapsed}");

            return _newAdMessages;
        }

        private async Task CheckExistenceOrPriceInDb(AdLink adLink)
        {
            await _semaphoreSlim.WaitAsync();

            try
            {
                using (var scopeDb = _serviceScopeFactory.CreateScope())
                {
                    var otoMotoContext = scopeDb.ServiceProvider.GetRequiredService<OtoMotoContext>();

                    var adLinkFromDb = await otoMotoContext.AdLinks.FirstOrDefaultAsync(x => x.Link == adLink.Link);

                    if (adLinkFromDb != null)
                    {
                        if (adLinkFromDb.Price != adLink.Price)
                        {
                            _newAdMessages.Add(new NewAdMessage()
                                {Link = adLink.Link, Price = adLink.Price, PriceBefore = adLinkFromDb.Price});

                            adLinkFromDb.Price = adLink.Price;

                            lock (_locker)
                                UpdatedPriceRecords++;
                        }
                    }
                    else
                    {
                        _newAdMessages.Add(new NewAdMessage() {Link = adLink.Link, Price = adLink.Price});

                        await otoMotoContext.AdLinks.AddAsync(adLink);

                        lock (_locker)
                            NewRecords++;
                    }

                    await otoMotoContext.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
