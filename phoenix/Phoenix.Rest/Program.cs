using System.Threading.Tasks;
using ConfigurationManager;
using Core.Metrics;
using Core.Middleware;
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
            Metrics.CollectDefault();
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/phoenix/{apiVersion}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);

            ConfigurationManager.ApplicationConfiguration.Init();

            var host = KalturaWebHostBuilder.BuildWebServerAsync<Startup>(new WebServerConfiguration
            {
                CommandlineArgs = args,
                AllowSynchronousIO = true,
                DefaultLogDirectoryPath = defaultLogDir,
                //to bytes
                MaxRequestBodySize = ApplicationConfiguration.Current.KestrelConfiguration.MaxRequestBodySize.Value*1000*1000,
            });
            
            await host.RunAsync();
        }
    }
}
