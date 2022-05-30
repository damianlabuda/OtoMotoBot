using Redis.OM.Modeling;

namespace Telegram.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new []{ "TelegramCurrentAction" })]
    public class TelegramCurrentActionRedis
    {
        [RedisIdField]
        [Indexed]
        public long TelegramChatId { get; set; }
        public string CurrentAction { get; set; }
    }
}
