using System;
using System.Linq;
using Core.Users.Cache;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using RestSharp.Extensions;

namespace Phoenix.AsyncHandler
{
    public static class Startup
    {
        public static IHostBuilder ConfigureAsyncHandler(this IHostBuilder builder)
        {
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/ott-service-phoenix-async-handler/");
            ApplicationConfiguration.Init();

            builder
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddProvider(new KLoggerProvider());
                    logging.SetMinimumLevel(GetLogLevelFromKLogger());
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddKafkaConsumerFactory(KafkaConfig.Get());
                    services
                        .AddDependencies()
                        .AddKafkaHandlersFromAssembly()
                        .AddMetricsAndHealthHttpServer();
                });

            return builder;
        }

        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            // TODO looks like it's not possible to use "scoped" service without extra effort  
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio#consuming-a-scoped-service-in-a-background-task
            //services.AddScoped<IRequestContext, RequestContext>();

            services
                .AddSingleton(p => DomainsCache.Instance());
            
            return services;
        }

        public static IServiceCollection AddKafkaHandlersFromAssembly(this IServiceCollection services)
        {
            var assembly = typeof(Handler<>).Assembly;
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsSubclassOfRawGeneric(typeof(Handler<>)) && !t.IsAbstract);
            foreach (var handlerType in handlerTypes)
            {
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), handlerType));
            }

            return services;
        }

        public static IServiceCollection AddMetricsAndHealthHttpServer(this IServiceCollection services)
        {
            if (!int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var port)) port = 8080;

            return services.AddHostedService(serviceProvider =>
            {
                var logger = serviceProvider.GetService<ILogger<HttpHostedService>>();
                return new HttpHostedService(port, logger);
            });
        }

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
