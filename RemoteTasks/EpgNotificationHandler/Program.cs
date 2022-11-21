using System.Threading.Tasks;
using EpgNotificationHandler.Infrastructure;
using EventBus.RabbitMQ;
using KLogMonitor;
using log4net.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationHandlers.Common;
using WebAPI.Filters;

namespace EpgNotificationHandler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBusConsumer()
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new KLoggerProvider());
                    logging.SetMinimumLevel(GetLogLevelFromKLogger());
                })
                .ConfigureServices(services => services.AddEpgNotificationHandlerDependencies());

            AppMetrics.Start();

            await builder.RunConsoleAsync();
        }
        
        // TODO it's a copy-paste, should be Phx.Lib.Log
        private static LogLevel GetLogLevelFromKLogger()
        {
            var level = KLogger.GetLogLevel();
            if (level >= Level.Error) return LogLevel.Error;
            if (level >= Level.Warn) return LogLevel.Warning;
            if (level >= Level.Info) return LogLevel.Information;
            if (level >= Level.Debug) return LogLevel.Debug;
            return LogLevel.Trace;
        }
    }
}
