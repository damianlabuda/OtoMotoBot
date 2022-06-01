using Redis.OM;
using Telegram.Models;

namespace Telegram.HostedServices
{
    public class RedisIndexCreationService : IHostedService
    {
        private readonly RedisConnectionProvider _redis;

        public RedisIndexCreationService(RedisConnectionProvider redis)
        {
            _redis = redis;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var listOfIndexes = (await _redis.Connection.ExecuteAsync("FT._LIST")).ToArray().Select(x => x.ToString());
            if (listOfIndexes.All(x => x != "telegramcurrentactionredis-idx"))
            {
                await _redis.Connection.CreateIndexAsync(typeof(TelegramCurrentActionRedis));
            }

            if (listOfIndexes.All(x => x != "telegramtimelastactionredis-idx"))
            {
                await _redis.Connection.CreateIndexAsync(typeof(TelegramTimeLastActionRedis));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
