using Shared.Entities;

namespace Shared.Models;

public class TelegramMessagesToSend
{
    public string Message { get; set; }
    public List<User> Users { get; set; }
}