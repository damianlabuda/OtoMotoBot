using Sender;
using Sender.Interfaces;
using Sender.Services;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHttpClient("TelegramSender")
            .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(hostContext.Configuration.GetConnectionString("TelegramToken"), client));

        services.AddScoped<ITelegramSenderService, TelegramSenderService>();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
