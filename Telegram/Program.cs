using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;
using Telegram.Bot;
using Telegram.Commands;
using Telegram.HostedServices;
using Telegram.Interfaces;
using Telegram.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionStrings = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ICommandExecutorService, CommandExecutorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISearchLinkService, SearchLinkService>();
builder.Services.AddScoped<IDefaultCommand, DefaultCommand>();
builder.Services.AddScoped<IBaseCommand, DefaultCommand>();
builder.Services.AddScoped<IBaseCommand, AddLinkCommand>();
builder.Services.AddScoped<IBaseCommand, ShowMyLinksCommand>();
builder.Services.AddScoped<IBaseCommand, OptionsLinkCommand>();
builder.Services.AddScoped<IBaseCommand, RemoveLinkCommand>();

builder.Services.AddHostedService<TelegramWebhookService>();
builder.Services.AddHttpClient("tgwebhook").AddTypedClient<ITelegramBotClient>(httpClient =>
    new TelegramBotClient(connectionStrings.TelegramToken, httpClient));

builder.Services.AddSingleton(new RedisConnectionProvider(connectionStrings.RedisConnectionString));
builder.Services.AddHostedService<RedisIndexCreationService>();

builder.Services.AddDbContext<OtoMotoContext>(options =>
    options.UseNpgsql(connectionStrings.OtoMotoDbConnectionString));

var app = builder.Build();

// app.UseHttpsRedirection();

app.UseRouting();

// app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "tgwebhook",
        pattern: $"webhook/{connectionStrings.TelegramToken}",
        new { controller = "TelegramBot", action = "Post" });
    endpoints.MapControllers();
});

app.Run();
