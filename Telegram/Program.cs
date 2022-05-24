using Microsoft.EntityFrameworkCore;
using Telegram.Services;
using Shared.Entities;
using Telegram;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<TelegramBot>();
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddScoped<ICommandExecutorServices, CommandExecutorServices>();

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
