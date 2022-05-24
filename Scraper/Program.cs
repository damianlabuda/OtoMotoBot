using Microsoft.EntityFrameworkCore;
using Scraper;
using Shared.Entities;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var optionsBuilder = new DbContextOptionsBuilder<OtoMotoContext>();
        optionsBuilder.UseSqlServer(hostContext.Configuration.GetConnectionString("OtoMotoTestConnectionString"));
        services.AddScoped(x => new OtoMotoContext(optionsBuilder.Options));

        services.AddHttpClient<IOtoMotoHttpClient, OtoMotoHttpClient>(options =>
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
            UseCookies = false
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
