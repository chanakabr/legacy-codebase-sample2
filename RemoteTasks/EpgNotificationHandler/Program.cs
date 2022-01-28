using System.Threading.Tasks;
using EpgNotificationHandler.Infrastructure;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;
using NotificationHandlers.Common;
using WebAPI.Filters;

namespace EpgNotificationHandler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBusConsumer()
                .ConfigureServices(serviceCollection => serviceCollection.AddEpgNotificationHandlerDependencies());

            AppMetrics.Start();

            await builder.RunConsoleAsync();
        }
    }
}
