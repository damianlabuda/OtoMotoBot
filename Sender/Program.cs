using MassTransit;
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
            x.AddConsumer<MessagesToSendQueueConsumerService>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ReceiveEndpoint("messagesToSend", e =>
                {
                    e.Consumer<MessagesToSendQueueConsumerService>(context);
                    e.ExchangeType = "direct";
                    e.Durable = false;
                });
            });
        });

    })
    .Build();

await host.RunAsync();
