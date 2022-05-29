using Microsoft.EntityFrameworkCore;
using Redis.OM;
using Telegram.Services;
using Shared.Entities;
using Telegram;
using Telegram.Commands;
using Telegram.HostedServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddScoped<ICommandExecutorServices, CommandExecutorService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<BaseCommand, DefaultCommand>();
builder.Services.AddScoped<IDefaultCommand, DefaultCommand>();
builder.Services.AddScoped<BaseCommand, AddLinkCommand>();
builder.Services.AddScoped<BaseCommand, ShowMyLinksCommand>();
builder.Services.AddScoped<BaseCommand, OptionsLinkCommand>();
builder.Services.AddScoped<BaseCommand, RemoveLinkCommand>();
builder.Services.AddSingleton(new RedisConnectionProvider(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddHostedService<IndexCreationService>();

builder.Services.AddDbContext<OtoMotoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OtoMotoTestConnectionString")));

var app = builder.Build();

// Telegram bot client
app.Services.GetRequiredService<TelegramBot>().GetBot().Wait();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
