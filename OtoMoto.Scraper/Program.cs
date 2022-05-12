using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OtoMoto.Scraper.Entities;
using OtoMoto.Scraper.HttpClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OtoMoto.Scraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var services = serviceCollection.BuildServiceProvider();

            //UserSeeder seeder = new UserSeeder();
            //seeder.Seed();

            List<SearchLink> searchLinks;

            await using (var context = new OtoMotoDbContext())
            {
                searchLinks = await context.SearchLinks.Include(x => x.AdLinks).ToListAsync();
            }

            foreach (var searchLink in searchLinks)
            {
                var httpClient = services.GetRequiredService<IOtoMotoHttpClient>();
                var otomotoSearch = new OtomotoSearchAuctions(searchLink, httpClient);
                var adLink = await otomotoSearch.Search();

                if (adLink.Any())
                {
                    await AddToDb(adLink);
                }
            }
        }

        private static async Task AddToDb(List<AdLink> newAdLinks)
        {
            await using (var context = new OtoMotoDbContext())
            {
                List<AdLink> NewAdLinksToAdd = new List<AdLink>();

                foreach (var newAdLink in newAdLinks)
                {
                    var adLinkFromDb = await context.AdLinks.FirstOrDefaultAsync(x => x.Link == newAdLink.Link);
                    
                    if (adLinkFromDb != null)
                    {
                        if (adLinkFromDb.Price != newAdLink.Price)
                        {
                            adLinkFromDb.Price = newAdLink.Price;
                        }

                        continue;
                    }

                    NewAdLinksToAdd.Add(newAdLink);
                }

                if (NewAdLinksToAdd.Any())
                {
                    await context.AdLinks.AddRangeAsync(NewAdLinksToAdd);
                }

                await context.SaveChangesAsync();
            }
        }
        
        public static void ConfigureServices(ServiceCollection service)
        {
            service.AddHttpClient<IOtoMotoHttpClient, OtoMotoHttpClient>(options =>
            {
                options.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
                options.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"(Not(A:Brand\";v=\"8\", \"Chromium\";v=\"98\"");
                options.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
                options.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                options.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                options.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36");
                options.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                options.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                options.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                options.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                options.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                options.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                options.DefaultRequestHeaders.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler()
                {
                    //Proxy = new WebProxy("http://127.0.0.1:8080"),
                    //UseProxy = true,
                    UseCookies = false
                };
            });
        }
    }
}