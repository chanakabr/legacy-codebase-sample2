using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using EventBus.RabbitMQ;
using WebAPI.Filters;
using Microsoft.Extensions.DependencyInjection;
using Core.Notification;
using ApiLogic.Notification;
using EpgNotificationHandler.EpgCache;

namespace EpgNotificationHandler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBustConsumer()
                .ConfigureServices(services =>
                {
                    services
                        .AddScoped<IIotManager>(provider => IotManager.Instance)
                        .AddSingleton<INotificationCache>(provider => NotificationCache.Instance())
                        .AddEpgCacheClient();
                });

            AppMetrics.Start();

            await builder.RunConsoleAsync();
        }
    }
}
