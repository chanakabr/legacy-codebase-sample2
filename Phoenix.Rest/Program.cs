using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KLogMonitor;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Phoenix.Rest
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"C:\log\HealthCheck\{apiVersion}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);
            ConfigurationManager.ApplicationConfiguration.Initialize(shouldLoadDefaults: true, silent: true);
            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(o => o.AllowSynchronousIO = false)
                .UseStartup<Startup>();
    }
}
