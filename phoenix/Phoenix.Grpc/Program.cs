using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using OTT.Lib.GRPC.Server;

namespace Phoenix.Grpc
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var apiVersion = System.Configuration.ConfigurationManager.AppSettings.Get("apiVersion");
            var defaultLogDir = $@"/var/log/phoenix/{apiVersion}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, defaultLogDir);

            ApplicationConfiguration.Init();

            var host = GrpcServerHost.CreateHostBuilder(args)
                .UseStartup<Startup>()
                .Build();

            await host.RunAsync();
        }
    }
}
