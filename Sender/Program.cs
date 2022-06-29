using MassTransit;
using Microsoft.EntityFrameworkCore;
using Sender;
using Sender.Interfaces;
using Sender.Services;
using Shared.Entities;
using Shared.Models;
using Telegram.Bot;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var connectionStrings = hostContext.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

        services.AddDbContext<OtoMotoContext>(options =>
            options.UseNpgsql(connectionStrings.OtoMotoDbConnectionString));
        
        services.AddHttpClient("TelegramSender")
            .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(connectionStrings.TelegramToken, client));

        services.AddScoped<ITelegramSenderService, TelegramSenderService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Worker>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(connectionStrings.RabbitHost, "/", h =>
                {
                    h.Username(connectionStrings.RabbitUser);
                    h.Password(connectionStrings.RabbitPassword);
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
