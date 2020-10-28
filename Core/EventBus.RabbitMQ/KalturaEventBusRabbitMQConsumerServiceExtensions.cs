using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigurationManager;
using EventBus.Abstraction;
using KLogMonitor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Retry;

namespace EventBus.RabbitMQ
{
    public class EventBusConfiguration
    {
        public Func<IEnumerable<int>> DedicatedPartnerIdsResolver { get; set; }
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
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Assembly _EntryAssembly = Assembly.GetEntryAssembly();
        private static readonly Type[] _EntryAssemblyTypes = _EntryAssembly.GetTypes();
        private static readonly List<Type> _AllServiceHandlers = _EntryAssemblyTypes.Where(IsTypeImplementsIServiceHandler).ToList();
        private static EventBusConfiguration _Configuration;

        public static IHostBuilder ConfigureEventBustConsumer(this IHostBuilder builder)
        {
            ConfigureEventBustConsumer(builder, config => { });
            return builder;
        }

        public static IHostBuilder ConfigureEventBustConsumer(this IHostBuilder builder, Action<EventBusConfiguration> configureService)
        {
            InitLogger();
            ApplicationConfiguration.Init();
            Console.Title = _EntryAssembly.GetName().ToString();

            // Default configuration
            _Configuration = new EventBusConfiguration { };
            configureService(_Configuration);

            builder.ConfigureServices((hostContext, services) =>
            {
                // Add all discovered implementation of IServiceHandler as scoped services
                _AllServiceHandlers.ForEach(h => services.AddScoped(h));

                services.AddSingleton<IRabbitMQPersistentConnection>(RabbitMQPersistentConnection.GetInstanceUsingTCMConfiguration());
                ConfigureRabbitMQEventBus(services, _AllServiceHandlers);

                var isHealthy = HealthCheck(services);
                if (!isHealthy)
                {
                    throw new Exception("Health check returned errors, service will not start");
                }

                _Logger.Info($"Health check passed. Starting consumers.");
                services.AddHostedService<KalturaEventBusRabbitMQConsumerService>();
            });

            return builder;
        }

        private static bool HealthCheck(IServiceCollection services)
        {
            var retryCount = 3;
            var policy = Policy.Handle<Exception>()
                .WaitAndRetry(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time, attempt, ctx) => { _Logger.Warn($"Health check failed attempt:[{attempt}/{retryCount}]", ex); });

            _Logger.Info($"Starting health check.");
            _Logger.Info($"Checking couchbase connection...");

            var cbIsSuccess = false;
            var sqlIsSuccess = false;
            policy.Execute(() =>
            {
                var cb = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
                cbIsSuccess = cb.Set($"HealthCheckDoc_{_EntryAssembly.GetName()}", "", 1);
                if (!cbIsSuccess)
                {
                    throw new Exception("Could not get document from couchbase");
                }
            });


            _Logger.Info($"Checking SQL DB connection...");

            policy.Execute(() =>
            {
                var q = new ODBCWrapper.SelectQuery();
                q += "select 1";
                sqlIsSuccess = q.Execute();
                if (!sqlIsSuccess)
                {
                    throw new Exception("Could not query SQL DB");
                }
            });

            return (cbIsSuccess && sqlIsSuccess);
        }

        private static void InitLogger()
        {
            var assembly = Assembly.GetEntryAssembly();
            var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            var assemblyVersion = $"{fvi.FileMajorPart}_{fvi.FileMinorPart}_{fvi.FileBuildPart}";
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WindowsService, @"/var/log/EventHandlers/");
        }

        private static void ConfigureRabbitMQEventBus(IServiceCollection services, List<Type> allServiceHandlers)
        {
            services.AddSingleton((Func<IServiceProvider, IEventBusConsumer>) (serviceProvider =>
            {
                var queueName = _EntryAssembly.GetName().Name;
                var rabbitMQPersistentConnection = serviceProvider.GetRequiredService<IRabbitMQPersistentConnection>();
                var concurrentConsumersIntValue = GetConcurrentConsumersFromEnvironmentVars();
                var partnerIds = GetDedicatedConsumerPartnerIds();

                var eventBus = EventBusConsumerRabbitMQ.GetInstanceUsingTCMConfiguration(serviceProvider, rabbitMQPersistentConnection, queueName, concurrentConsumersIntValue, partnerIds);

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

        private static IEnumerable<int> GetDedicatedConsumerPartnerIds()
        {
            IEnumerable<int> partnerIds = null;
            if (_Configuration.DedicatedPartnerIdsResolver != null)
            {
                _Logger.Info($"Detected dedicated consumer requirement configuration, resolving partner Ids.");
                partnerIds = _Configuration.DedicatedPartnerIdsResolver();
                if (partnerIds?.Any() == true)
                {
                    _Logger.Info($"Dedicated consumers for Partners: [{string.Join(",", partnerIds)}]");
                }
                else
                {
                    _Logger.Warn($"Dedicated partner resolver have returned a Null or empty list of partners");
                }
            }

            return partnerIds;
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