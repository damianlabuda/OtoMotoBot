using MassTransit;
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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Worker>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitmq", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
            
                cfg.ReceiveEndpoint("messagesToSend", e =>
                {
                    e.Consumer<Worker>(context);
                    e.ExchangeType = "direct";
                    e.Durable = false;
                });
            });
        });

    })
    .Build();

await host.RunAsync();
