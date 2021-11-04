using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using EventBus.RabbitMQ;
using WebAPI.Filters;

namespace CampaignHandler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
              .ConfigureMappings()
              .ConfigureEventNotificationsConfig()
              .ConfigureEventBusConsumer();
            await builder.RunConsoleAsync();
        }
    }
}
