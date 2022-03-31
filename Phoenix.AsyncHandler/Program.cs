using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Phoenix.AsyncHandler
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder().ConfigureAsyncHandler();
            await builder.RunConsoleAsync();
        }
    }
}