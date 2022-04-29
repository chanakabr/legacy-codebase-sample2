using System;
using Core.Users.Cache;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.AsyncHandler.Pricing;
using Phoenix.Generated.Api.Events.Crud.Household;
using Phoenix.Generated.Api.Events.Logical.appstoreNotification;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;

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
            services.AddSingleton(p => DomainsCache.Instance());
            services.AddScoped<IKafkaContextProvider, AsyncHandlerKafkaContextProvider>();
            
            return services;
        }

        public static IServiceCollection AddKafkaHandlersFromAssembly(this IServiceCollection services)
        {
            services.AddKafkaHandler<HouseholdNpvrAccountHandler, Household>("household-npvr-account", Household.GetTopic());
            services.AddKafkaHandler<EntitlementLogicalHandler, AppstoreNotification>("appstore-notification", AppstoreNotification.GetTopic());
            return services;
        }

        private static IServiceCollection AddKafkaHandler<THandler, TValue>(this IServiceCollection services, string kafkaGroupSuffix, string topic) where THandler : IHandler<TValue>
        {
            services.AddScoped(typeof(THandler), typeof(THandler));
            services.AddSingleton<IHostedService, BackgroundServiceStarter<THandler, TValue>>(p =>
                new BackgroundServiceStarter<THandler, TValue>(p.GetService<IKafkaConsumerFactory>(), p.GetService<IServiceScopeFactory>(), kafkaGroupSuffix, topic));
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
