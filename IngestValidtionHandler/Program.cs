using System.Threading.Tasks;
using EventBus.RabbitMQ;
using Microsoft.Extensions.Hosting;

namespace IngestValidtionHandler
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureEventBustConsumer();
            await builder.RunConsoleAsync();
        }
    }
}
