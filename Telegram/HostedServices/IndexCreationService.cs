using Redis.OM;
using Shared.Models;

namespace Telegram.HostedServices
{
    public class IndexCreationService : IHostedService
    {
        private readonly RedisConnectionProvider _redis;

        public IndexCreationService(RedisConnectionProvider redis)
        {
            _redis = redis;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var listOfIndexes = (await _redis.Connection.ExecuteAsync("FT._LIST")).ToArray().Select(x => x.ToString());
            if (listOfIndexes.All(x => x != "telegramcurrentaction-idx"))
            {
                await _redis.Connection.CreateIndexAsync(typeof(TelegramCurrentAction));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
