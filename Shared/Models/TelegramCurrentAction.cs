using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Redis.OM.Modeling;

namespace Shared.Models
{
    [Document(StorageType = StorageType.Json, Prefixes = new []{ "TelegramCurrentAction" })]
    public class TelegramCurrentAction
    {
        [RedisIdField]
        [Indexed]
        public long TelegramChatId { get; set; }
        public string CurrentAction { get; set; }
    }
}
