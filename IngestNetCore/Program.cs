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

namespace IngetsNetCore
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            string apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"C:\log\ws-ingest\{apiVersion}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);
            ConfigurationManager.ApplicationConfiguration.Init();

            await CreateWebHostBuilder(args).Build().RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) => { logging.ClearProviders(); })
                //.ConfigureKestrel(o => o.AllowSynchronousIO = true)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();
    }
}
