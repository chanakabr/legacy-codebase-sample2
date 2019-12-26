using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Middleware;
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
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/ws-ingest/{apiVersion}";

            await KalturaWebHostBuilder.RunWebServerAsync<Startup>(new WebServerConfiguration
            {
                CommandlineArgs = args,
                DefaultLogDirectoryPath = defaultLogDir,
            });
        }
    }
}
