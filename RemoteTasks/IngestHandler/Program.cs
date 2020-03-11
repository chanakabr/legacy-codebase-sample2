using System.Threading.Tasks;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;
using WebAPI.Filters;

namespace EPGTransformationHandler
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBustConsumer();
            await builder.RunConsoleAsync();
        }
    }
}
