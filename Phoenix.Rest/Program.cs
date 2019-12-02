using System.Threading.Tasks;
using ConfigurationManager;
using KLogMonitor;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Phoenix.Rest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/phoenix/{apiVersion}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);
            
            ApplicationConfiguration.Init();
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) => { logging.ClearProviders(); })
                .ConfigureKestrel(o => o.AllowSynchronousIO = false)
                .UseStartup<Startup>();
    }
}
