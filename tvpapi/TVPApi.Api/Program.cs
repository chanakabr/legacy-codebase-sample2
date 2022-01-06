using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Phx.Lib.Appconfig;
using Core.Metrics;
using Core.Middleware;
using Phx.Lib.Log;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TVPApi.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Metrics.CollectDefault();
            
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/tvpapi/{apiVersion}";
            Phx.Lib.Log.KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);
            ApplicationConfiguration.Init();
            await KalturaWebHostBuilder.RunWebServerAsync<Startup>(new WebServerConfiguration
            {
                CommandlineArgs = args,
                AllowSynchronousIO = true,
                DefaultLogDirectoryPath = defaultLogDir,
            });
        }
    }
}
