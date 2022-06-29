using Coravel;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Entities;
using System.Threading.Tasks;
using Shared.Models;

namespace Scheduler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            host.Services.UseScheduler(sheduler =>
            {
                // Adds a job to check the auction every 5 minutes
                sheduler.Schedule<Worker>().EveryFiveMinutes();
                
                // Adds a task to check if the auction exists or if the price has changed
                
                
                // var jobShedule = sheduler.Schedule<Worker>();
                // jobShedule.EveryFiveMinutes();
            });

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var connectionStrings = hostContext.Configuration.GetSection("ConnectionStrings")
                        .Get<ConnectionStrings>();
                    
                    services.AddDbContext<OtoMotoContext>(options =>
                        options.UseNpgsql(connectionStrings.OtoMotoDbConnectionString));

                    services.AddMassTransit(x =>
                    {
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(connectionStrings.RabbitHost, "/", h =>
                            {
                                h.Username(connectionStrings.RabbitUser);
                                h.Password(connectionStrings.RabbitPassword);
                            });
                        });
                    });

                    services.AddScheduler();
                    services.AddTransient<Worker>();
                });
    }
}
