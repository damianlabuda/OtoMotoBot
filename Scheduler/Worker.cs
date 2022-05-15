using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Shared.Entities;
using Newtonsoft.Json;
using System.Text;
using RabbitMQ.Client.Exceptions;

namespace Scheduler
{
    public class Worker : IInvocable
    {
        private readonly ILogger<Worker> _logger;

        private readonly OtoMotoContext _dbOtoMotoContext;

        public Worker(ILogger<Worker> logger, OtoMotoContext dbOtoMotoContext)
        {
            _logger = logger;
            _dbOtoMotoContext = dbOtoMotoContext;
        }

        public async Task Invoke()
        {
            try
            {
                var factory = new ConnectionFactory() {HostName = "localhost"};
                using (var connection = factory.CreateConnection())
                using (var channelSearchLinks = connection.CreateModel())
                {
                    channelSearchLinks.QueueDeclare(queue: "searchLinks", durable: false, exclusive: false,
                        autoDelete: false, arguments: null);

                    var searchLinks = await _dbOtoMotoContext.SearchLinks.Include(x => x.Users).ToListAsync();

                    foreach (var searchLink in searchLinks)
                    {
                        var searchLinkDto = new SearchLink()
                        {
                            Id = searchLink.Id,
                            AdLinks = searchLink.AdLinks,
                            CreatedDateTime = searchLink.CreatedDateTime,
                            LastUpdateDateTime = searchLink.LastUpdateDateTime,
                            Link = searchLink.Link,
                            SearchCount = searchLink.SearchCount,
                            Users = searchLink.Users.Select(x => new User()
                            {
                                CreatedDateTime = x.CreatedDateTime,
                                Id = x.Id,
                                LastUpdateDateTime = x.LastUpdateDateTime,
                                TelegramChatId = x.TelegramChatId,
                                TelegramName = x.TelegramName
                            }).ToList()
                        };

                        var json = JsonConvert.SerializeObject(searchLinkDto);

                        var body = Encoding.UTF8.GetBytes(json);

                        channelSearchLinks.BasicPublish(exchange: "", "searchLinks", mandatory: false,
                            basicProperties: null, body);

                        searchLink.SearchCount++;

                        _logger.LogInformation($"Dodano link wyszukiwania do kolejki: {searchLink.Link}");
                    }
                }

                await _dbOtoMotoContext.SaveChangesAsync();
            }
            catch (AlreadyClosedException)
            {
                _logger.LogError($"Problem with RabbitMQ connection");
            }
            catch (JsonException e)
            {
                _logger.LogError($"Json parse error: {e.Message}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception: {e.Message}");
            }
        }
    }
}
