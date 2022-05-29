using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Shared.Models;
using System.Diagnostics;

namespace Scraper
{
    public class SearchInDb
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(100, 100);

        private readonly List<NewAdMessage> _newAdMessages = new List<NewAdMessage>();

        private readonly object locker = new object();

        public int NewRecords { get; set; } = 0;
        public int UpdatedPriceRecords { get; set; } = 0;

        public SearchInDb(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<List<NewAdMessage>> Check(List<AdLink> adLinks)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            var tasks = adLinks.Select(CheckExistenceOrPriceInDb);

            await Task.WhenAll(tasks);

            stopwatch.Stop();

            Console.WriteLine($"{DateTime.Now} - {NewRecords} nowych rekordów, {UpdatedPriceRecords} zmian cen, czas: {stopwatch.Elapsed}");

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
                            _newAdMessages.Add(new NewAdMessage(){ Link = adLink.Link, Price = adLink.Price, PriceBefore = adLinkFromDb.Price });

                            adLinkFromDb.Price = adLink.Price;

                            lock (locker)
                                UpdatedPriceRecords++;
                        }
                    }
                    else
                    {
                        _newAdMessages.Add(new NewAdMessage() { Link = adLink.Link, Price = adLink.Price });

                        await otoMotoContext.AdLinks.AddAsync(adLink);

                        lock (locker)
                            NewRecords++;
                    }

                    await otoMotoContext.SaveChangesAsync();
                }
            }
            catch (Exception) { }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
