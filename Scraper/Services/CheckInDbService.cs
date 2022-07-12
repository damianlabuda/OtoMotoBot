using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scraper.Interfaces;
using Shared.Entities;
using Shared.Models;

namespace Scraper.Services
{
    public class CheckInDbService : ICheckInDbService
    {
        private readonly OtoMotoContext _otoMotoContext;
        private readonly ILogger<CheckInDbService> _logger;
        private readonly List<NewAdMessage> _newAdMessages = new();
        private int NewRecords { get; set; }
        private int UpdatedPriceRecords { get; set; }
        private int DeletedRecords { get; set; }
        private int CheckedRecords { get; set; }
        private SearchLink _searchLink = new();

        public CheckInDbService(OtoMotoContext otoMotoContext, ILogger<CheckInDbService> logger)
        {
            _otoMotoContext = otoMotoContext;
            _logger = logger;
        }

        public async Task<List<NewAdMessage>> Check(List<AdLink> adLinks, SearchLink searchLink)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            _searchLink = await _otoMotoContext.SearchLinks.Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == searchLink.Id);

            if (_searchLink != null)
            {
                if (_searchLink.SearchCount != 0 && _searchLink.SearchCount % 10 == 0)
                {
                    await CheckAllAdsInDbForExistAndPrice(adLinks);
                    _searchLink.AdLinksCount = adLinks.Count();
                }
                else
                {
                    await CheckNewAds(adLinks);
                }

                if (_searchLink.SearchCount == 0)
                    _searchLink.AdLinksCount = adLinks.Count();
                
                if (CheckedRecords > 0)
                    _searchLink.SearchCount++;

                await _otoMotoContext.SaveChangesAsync();
            }

            stopwatch.Stop();

            _logger.LogInformation(
                $"{DateTime.Now} - {NewRecords} nowych rekordów, " +
                $"{UpdatedPriceRecords} zmian cen, " +
                $"usunietych rekordow: {DeletedRecords}, " +
                $"suma sprawdzonych rekordów: {CheckedRecords}," +
                $"czas: {stopwatch.Elapsed}");

            return _newAdMessages;
        }

        private async Task CheckAllAdsInDbForExistAndPrice(List<AdLink> adLinks)
        {
            var allAdLinksForSearchLink = await _otoMotoContext.AdLinks
                .Include(x => x.SearchLinks).ThenInclude(x => x.Users)
                .Include(x => x.Prices)
                .Where(x => x.SearchLinks.Contains(_searchLink))
                .ToListAsync();

            if (!allAdLinksForSearchLink.Any())
                return;

            foreach (var adLinkFromDb in allAdLinksForSearchLink)
            {
                if (!adLinks.Select(x => x.Id).ToList().Contains(adLinkFromDb.Id))
                {
                    adLinkFromDb.HowManyTimesHasNotInSearch++;
                    continue;
                }

                var adlinkFromSearch = adLinks.FirstOrDefault(x => x.Id == adLinkFromDb.Id);

                if (adlinkFromSearch == null)
                    continue;

                if (adLinkFromDb.Prices.OrderByDescending(s => s.CreatedDateTime).Select(s => s.Price).FirstOrDefault()
                    != adlinkFromSearch.Prices.Select(p => p.Price).FirstOrDefault()
                    || adLinkFromDb.Prices.OrderByDescending(s => s.CreatedDateTime).Select(c => c.Currency).FirstOrDefault() 
                    != adlinkFromSearch.Prices.Select(c => c.Currency).FirstOrDefault())
                {
                    var users = adLinkFromDb.SearchLinks
                        .SelectMany(x => x.Users)
                        .Select(x => new User()
                        {
                            Id = x.Id,
                            TelegramChatId = x.TelegramChatId,
                            TelegramName = x.TelegramName
                        }).ToList();

                    _newAdMessages.Add(new NewAdMessage()
                    {
                        Id = adlinkFromSearch.Id,
                        Price = adlinkFromSearch.Prices.Select(p => p.Price).FirstOrDefault(),
                        Currency = adlinkFromSearch.Prices.Select(c => c.Currency).FirstOrDefault(),
                        PriceBefore = adLinkFromDb.Prices.OrderByDescending(s => s.CreatedDateTime).Select(p => p.Price).FirstOrDefault(),
                        CurrencyBefore = adLinkFromDb.Prices.OrderByDescending(s => s.CreatedDateTime).Select(c => c.Currency).FirstOrDefault(),
                        Users = users
                    });

                    adLinkFromDb.Prices.AddRange(adlinkFromSearch.Prices);

                    UpdatedPriceRecords++;
                }

                adLinkFromDb.HowManyTimesHasNotInSearch = 0;
            }

            _otoMotoContext.AdLinks.RemoveRange(
                allAdLinksForSearchLink.Where(x => x.HowManyTimesHasNotInSearch >= 3));
            DeletedRecords = allAdLinksForSearchLink.Count(x => x.HowManyTimesHasNotInSearch >= 3);
            CheckedRecords = allAdLinksForSearchLink.Count();
            
            await _otoMotoContext.SaveChangesAsync();
        }

        private async Task CheckNewAds(List<AdLink> adLinks)
        {
            var idsForSearchInDb = adLinks.Select(x => x.Id).Distinct().ToList();

            var adLinksFromDb = await _otoMotoContext.AdLinks.Include(x => x.SearchLinks)
                .Where(t => idsForSearchInDb.Contains(t.Id)).ToListAsync();

            // var adLinksForCheckExistSearchLinks =
            //     adLinksFromDb.Where(x => adLinks.Select(s => s.Id).Contains(x.Id)).ToList();
            // if (adLinksForCheckExistSearchLinks.Any())
            if (adLinksFromDb.Any())
            {
                // var adLinksForAddSearchLink = adLinksForCheckExistSearchLinks
                //     .Where(x => !x.SearchLinks.Select(s => s.Id).Contains(_searchLink.Id)).ToList();

                var adLinksForAddSearchLink = adLinksFromDb
                    .Where(x => !x.SearchLinks.Select(s => s.Id).Contains(_searchLink.Id)).ToList();
                
                if (adLinksForAddSearchLink.Any())
                {
                    adLinksForAddSearchLink.ForEach(x => x.SearchLinks.Add(_searchLink));

                    var newAdMessages = adLinksForAddSearchLink.Select(x => new NewAdMessage()
                    {
                        Id = x.Id,
                        Price = x.Prices.Select(p => p.Price).FirstOrDefault(),
                        Currency = x.Prices.Select(c => c.Currency).FirstOrDefault(),
                        Users = _searchLink.Users.Select(u => new User()
                        {
                            Id = u.Id,
                            TelegramChatId = u.TelegramChatId,
                            TelegramName = u.TelegramName
                        }).ToList()
                        
                    }).ToList();
                    _newAdMessages.AddRange(newAdMessages);

                    NewRecords += adLinksForAddSearchLink.Count;
                }
            }

            var newAdLinksToAdd = adLinks.Where(x => !adLinksFromDb.Select(s => s.Id).Contains(x.Id)).ToList();
            if (newAdLinksToAdd.Any())
            {
                newAdLinksToAdd.ForEach(x => x.SearchLinks = new List<SearchLink> {_searchLink});
                await _otoMotoContext.AdLinks.AddRangeAsync(newAdLinksToAdd);

                var newAdMessages = newAdLinksToAdd.Select(x => new NewAdMessage()
                {
                    Id = x.Id,
                    Price = x.Prices.Select(p => p.Price).FirstOrDefault(),
                    Currency = x.Prices.Select(c => c.Currency).FirstOrDefault(),
                    Users = _searchLink.Users.Select(u => new User()
                    {
                        Id = u.Id,
                        TelegramChatId = u.TelegramChatId,
                        TelegramName = u.TelegramName
                    }).ToList()
                }).ToList();
                _newAdMessages.AddRange(newAdMessages);

                NewRecords += newAdLinksToAdd.Count;
            }

            CheckedRecords = idsForSearchInDb.Count();
            
            await _otoMotoContext.SaveChangesAsync();
        }
    }
}