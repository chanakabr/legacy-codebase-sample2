using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using EventBus.Abstraction;
using KLogMonitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventBus.RabbitMQ
{
    // TODO: is there a service handler configuration required here ? 
    public class KalturaMicroserviceConfiguration
    {
        public string HandlerUniqueId { get; set; }
        public int ConcurrentConsumers { get; set; }
    }

    public class RabbitMQConnectionDetails
    {
        public string HostName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }

    public static class KalturaEventBusRabbitMQConsumerServiceExtensions
    {
        private static readonly Assembly _EntryAssembly = Assembly.GetEntryAssembly();
        private static readonly Type[] _EntryAssemblyTypes = _EntryAssembly.GetTypes();
        private static readonly List<Type> _AllServiceHandlers = _EntryAssemblyTypes.Where(IsTypeImplementsIServiceHandler).ToList();

        public static IHostBuilder ConfigureEventBustConsumer(this IHostBuilder builder)
        {
            ConfigureEventBustConsumer(builder, config => { });
            return builder;
        }

        public static IHostBuilder ConfigureEventBustConsumer(this IHostBuilder builder, Action<KalturaMicroserviceConfiguration> configureService)
        {
            // Default configuration
            var configuration = new KalturaMicroserviceConfiguration
            {

            };

            configureService(configuration);
            InitLogger();

            builder.ConfigureServices((hostContext, services) =>
            {
                ApplicationConfiguration.Initialize();
                // Add all discovered implementation of IServiceHandler as scoped services
                foreach (var handler in _AllServiceHandlers)
                {
                    services.AddScoped(handler);
                }

                services.AddSingleton<IRabbitMQPersistentConnection>(RabbitMQPersistentConnection.GetInstanceUsingTCMConfiguration());
                ConfigureRabbitMQEventBus(services, configuration, _AllServiceHandlers);
                services.AddHostedService<KalturaEventBusRabbitMQConsumerService>();
            });

            return builder;
        }

        private static void InitLogger()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyVersion = $"{fvi.FileMajorPart}_{fvi.FileMinorPart}_{fvi.FileBuildPart}";
            var logDir = Environment.GetEnvironmentVariable("API_LOG_DIR");
            logDir = logDir != null ? Environment.ExpandEnvironmentVariables(logDir) : @"C:\log\EventHandlers\";
            log4net.GlobalContext.Properties["LogDir"] = logDir;
            log4net.GlobalContext.Properties["ApiVersion"] = assemblyVersion;
            log4net.GlobalContext.Properties["LogName"] = assembly.GetName().Name;

            KMonitor.Configure("log4net.config", KLogEnums.AppType.WindowsService);
            KLogger.Configure("log4net.config", KLogEnums.AppType.WindowsService);

        }

        private static void ConfigureRabbitMQEventBus(IServiceCollection services, KalturaMicroserviceConfiguration configuration, List<Type> allServiceHandlers)
        {
            services.AddSingleton((Func<IServiceProvider, IEventBusConsumer>)(serviceProvider =>
            {
                var queueName = _EntryAssembly.GetName().Name;
                var rabbitMQPersistentConnection = serviceProvider.GetRequiredService<IRabbitMQPersistentConnection>();
                int concurrentConsumersIntValue = GetConcurrentConsumersFromEnvironmentVars();

                var eventBus = EventBusConsumerRabbitMQ.GetInstanceUsingTCMConfiguration(serviceProvider, rabbitMQPersistentConnection, queueName, concurrentConsumersIntValue);

                foreach (var handler in allServiceHandlers)
                {
                    var eventType = handler.GetInterfaces()
                        .First(IsInterfaceAnyGenericOfIServiceHandler)
                        .GetGenericArguments()
                        .First();
                    eventBus.Subscribe(eventType, handler);
                }

                return eventBus;
            }));
        }

        private static int GetConcurrentConsumersFromEnvironmentVars()
        {
            var concurrentConsumers = Environment.GetEnvironmentVariable("CONCURRENT_CONSUMERS");
            concurrentConsumers = concurrentConsumers != null ? Environment.ExpandEnvironmentVariables(concurrentConsumers) : "4";
            if (!int.TryParse(concurrentConsumers, out var concurrentConsumersIntValue))
            {
                concurrentConsumersIntValue = 4;
            }

            return concurrentConsumersIntValue;
        }

        private static bool IsInterfaceAnyGenericOfIServiceHandler(Type i)
        {
            return i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IServiceEventHandler<>);
        }

        private static bool IsTypeImplementsIServiceHandler(Type t)
        {
            var implementedInterfaces = t.GetInterfaces();
            return implementedInterfaces.Any(IsInterfaceAnyGenericOfIServiceHandler);
        }
    }
}