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
                    c.QueueName = "epg_ingest_transformation";
                    c.ConcurrentConsumers = 3;
                });
            await builder.RunConsoleAsync();

        }
    }
}
