using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Entities;
using Shared.Models;

namespace Scraper
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOtoMotoHttpClient _otoMotoHttpClient;

        public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory, IOtoMotoHttpClient otoMotoHttpClient)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _otoMotoHttpClient = otoMotoHttpClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //UserSeeder seeder = new UserSeeder(_serviceScopeFactory);
            //seeder.Seed();

            SearchLink searchLink;

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OtoMotoContext>();
                
                searchLink = context.SearchLinks
                    .Include(x => x.Users)
                    .FirstOrDefault();
            }

            var otomotoSearch = new OtomotoSearchAuctions(searchLink, _otoMotoHttpClient);
            var adLinks = await otomotoSearch.Search();

            if (adLinks.Any())
            {
                var searchInDb = new SearchInDb(_serviceScopeFactory);
                var newAdMessages = await searchInDb.Check(adLinks);

                if (newAdMessages.Any() /*&& searchLink.SearchCount > 0*/)
                {
                    MessagesToSent messagesToSent = new MessagesToSent()
                    {
                        NewAdMessages = newAdMessages,
                        Users = searchLink.Users.Select(x => new User{ TelegramChatId = x.TelegramChatId }).ToList<User>()
                    };

                    var json = JsonConvert.SerializeObject(messagesToSent);

                    Console.WriteLine(json);
                }
            }
        }
    }
}