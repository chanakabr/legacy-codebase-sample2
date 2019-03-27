using System.Threading.Tasks;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace EPGTransformationHandler
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureEventBustConsumer(c =>
                {
                    c.HandlerUniqueId = "13232";
                    c.ConcurrentConsumers = 3;
                });
            await builder.RunConsoleAsync();

        }
    }
}
