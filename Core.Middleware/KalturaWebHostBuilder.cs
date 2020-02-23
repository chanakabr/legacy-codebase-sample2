using KLogMonitor;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Middleware
{
    public static class KalturaWebHostBuilder
    {
        public static Task RunWebServerAsync<TStartup>(WebServerConfiguration config) where TStartup : class
        {
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, config.DefaultLogDirectoryPath);
            var webHost = WebHost.CreateDefaultBuilder(config.CommandlineArgs)
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new KLoggerProvider());
                })
                .ConfigureKestrel(o => o.AllowSynchronousIO = config.AllowSynchronousIO)
                .UseStartup<TStartup>()
                .Build();

            return webHost.RunAsync();
        }
    }

    public class WebServerConfiguration
    {
        public string[] CommandlineArgs { get; set; }
        public string DefaultLogDirectoryPath { get; set; }
        public bool AllowSynchronousIO { get; set; }
    }
}
