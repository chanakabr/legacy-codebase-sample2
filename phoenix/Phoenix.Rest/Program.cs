using System.Threading.Tasks;
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
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/phoenix/{apiVersion}";
            
            ConfigurationManager.ApplicationConfiguration.Init();

            await KalturaWebHostBuilder.RunWebServerAsync<Startup>(new WebServerConfiguration
            {
                CommandlineArgs = args,
                AllowSynchronousIO = true,
                DefaultLogDirectoryPath = defaultLogDir,
            });

        }
    }
}
