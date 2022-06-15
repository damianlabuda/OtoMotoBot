namespace Shared.Models;

public class ConnectionStrings
{
    public string OtoMotoDbConnectionString { get; set; }
    public string RedisConnectionString { get; set; }
    public string RabbitHost { get; set; }
    public string RabbitUser { get; set; }
    public string RabbitPassword { get; set; }
    public string TelegramToken { get; set; }
    public string UrlTelegramWebHook { get; set; }
}