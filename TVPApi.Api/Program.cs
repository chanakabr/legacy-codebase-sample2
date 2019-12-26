using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConfigurationManager;
using Core.Middleware;
using KLogMonitor;
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
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/tvpapi/{apiVersion}";

            await KalturaWebHostBuilder.RunWebServerAsync<Startup>(new WebServerConfiguration
            {
                CommandlineArgs = args,
                AllowSynchronousIO = true,
                DefaultLogDirectoryPath = defaultLogDir,
            });

        }
    }
}
