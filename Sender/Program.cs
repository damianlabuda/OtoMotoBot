using Sender;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("TelegramSender")
            .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(hostContext.Configuration.GetConnectionString("TelegramToken"), client));

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
