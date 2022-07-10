using System.Net;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scraper.Consumers;
using Scraper.Interfaces;
using Scraper.Services;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var connectionStrings = hostContext.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
        
        services.AddDbContext<OtoMotoContext>(options =>
            options.UseNpgsql(connectionStrings.OtoMotoDbConnectionString));

        services.AddScoped<ISearchAuctionsService, SearchAuctionsService>();
        services.AddScoped<ICheckInDbService, CheckInDbService>();
        services.AddScoped<ISearchLinkService, SearchLinkService>();
        services.AddScoped<IAdLinksService, AdLinksService>();
        
        services.AddHttpClient("OtomotoHttpClient", options =>
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
        }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
        {
            //Proxy = new WebProxy("http://127.0.0.1:8080"),
            //UseProxy = true,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All
        });

        services.AddMassTransit(x =>
        {
            x.AddConsumer<CheckSearchLinksConsumer>();
            x.AddConsumer<CheckAdCountConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionStrings.RabbitHost, "/", h => {
                    h.Username(connectionStrings.RabbitUser);
                    h.Password(connectionStrings.RabbitPassword);
                });

                cfg.ReceiveEndpoint("checkSearchLinks", e =>
                {
                    e.Consumer<CheckSearchLinksConsumer>(context);
                    e.ExchangeType = "direct";
                    e.Durable = false;
                });
                
                cfg.ReceiveEndpoint("checkAdCount", e =>
                {
                    e.Consumer<CheckAdCountConsumer>(context);
                    e.ExchangeType = "direct";
                    e.Durable = false;
                });
            });
        });

    })
    .Build();

await host.RunAsync();
