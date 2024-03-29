﻿using Redis.OM.Modeling;

namespace Telegram.Models
{
    [Document(StorageType = StorageType.Json)]
    public class TelegramCurrentActionRedis
    {
        [RedisIdField]
        [Indexed]
        public long TelegramChatId { get; set; }
        public string CurrentAction { get; set; }
    }
}
