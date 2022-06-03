using Redis.OM.Modeling;

namespace Telegram.Models
{
    [Document(StorageType = StorageType.Json)]
    public class TelegramTimeLastActionRedis
    {
        [RedisIdField]
        [Indexed]
        public long TelegramChatId { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
