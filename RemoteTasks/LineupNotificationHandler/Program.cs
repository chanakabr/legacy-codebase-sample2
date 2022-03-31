using System.Threading.Tasks;
using EventBus.RabbitMQ;
using LineupNotificationHandler.Infrastructure;
using Microsoft.Extensions.Hosting;
using NotificationHandlers.Common;
using WebAPI.Filters;

namespace LineupNotificationHandler
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBusConsumer()
                .ConfigureServices(services => services.AddLineupNotificationHandlerDependencies());

            AppMetrics.Start();

            await builder.RunConsoleAsync();
        }
    }
}