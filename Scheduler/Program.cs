using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Coravel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared.Entities;

namespace Scheduler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            host.Services.UseScheduler(sheduler =>
            {
                var jobShedule = sheduler.Schedule<Worker>();
                jobShedule.EveryFiveMinutes();
            });

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<OtomotoSearchAuctions>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetConnectionString("OtoMotoTestConnectionString")));

                    services.AddScheduler();
                    services.AddTransient<Worker>();
                });
    }
}
