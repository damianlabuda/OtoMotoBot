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
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            using (var channelMessagesToSend = connection.CreateModel())
            {
                channelMessagesToSend.QueueDeclare(queue: "messagesToSend", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var searchLinks = await _dbOtoMotoContext.SearchLinks.Include(x => x.Users).ToListAsync();

                foreach (var searchLink in searchLinks)
                {
                    var json = JsonConvert.SerializeObject(searchLink);

                    var body = Encoding.UTF8.GetBytes(json);

                    channelMessagesToSend.BasicPublish(exchange: "", "messagesToSend", mandatory: false, basicProperties: null, body);
                }
            }

        }
    }
}
