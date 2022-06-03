using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Shared.Entities;
using Shared.Interfaces;
using Shared.Services;
using Telegram.Bot;
using Telegram.Commands;
using Telegram.HostedServices;
using Telegram.Interfaces;
using Telegram.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
    new TelegramBotClient(builder.Configuration.GetConnectionString("TelegramToken"), httpClient));

builder.Services.AddSingleton(new RedisConnectionProvider(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddHostedService<RedisIndexCreationService>();

builder.Services.AddDbContext<OtoMotoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OtoMotoTestConnectionString")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(name: "tgwebhook",
        pattern: $"api/telegram/update/{builder.Configuration.GetConnectionString("TelegramToken")}",
        new { controller = "TelegramBot", action = "Post" });
    endpoints.MapControllers();
});

app.Run();
