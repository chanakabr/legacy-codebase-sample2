using System.Threading.Tasks;
using ApiLogic.IndexManager.Mappings;
using ApiObjects;
using Core.Catalog.CatalogManagement.Services;
using Core.Metrics;
using EventBus.Abstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using FeatureFlag;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Kafka;
using IngestTransformationHandler.Managers;
using IngestTransformationHandler.Repositories;
using Microsoft.Extensions.Configuration;
using Phx.Lib.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ott.Lib.FeatureToggle.IocExtensions;
using OTT.Lib.Kafka;
using TvinciCache;
using WebAPI.Filters;

namespace EPGTransformationHandler
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            Metrics.CollectDefaultAndStartServer();
            
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureServices(s =>
                {
                    s.AddScoped<IEpgRepository, EpgRepository>();
                    s.AddScoped<IEpgCRUDOperationsManager, EpgCRUDOperationsManager>();
                    s.AddScoped<IMappingTypeResolver, MappingTypeResolver>();
                    s.AddSingleton<ICatalogManagerAdapter, CatalogManagerAdapter>();
                    s.AddSingleton<IndexCompactionManager, IndexCompactionManager>();
                    s.AddKafkaProducerFactory(KafkaConfig.Get());
                    s.AddKafkaContextProvider<IngestKafkaContextProvider>();
                    s.AddScoped<IEventBusPublisher, KafkaPublisher>();
                    s.AddScoped<IEpgIngestMessaging>(provider => new EpgIngestMessaging(provider.GetService<IEventBusPublisher>(), new KLogger(nameof(EpgIngestMessaging))));
                    
                    s.AddFeatureToggle(new ConfigurationBuilder().AddEnvironmentVariables().Build());
                    s.AddScoped<IFeatureFlagContext, FeatureFlagIngestContext>();
                    s.AddScoped<IPhoenixFeatureFlag, PhoenixFeatureFlag>();
                })
                .ConfigureEventBusConsumer(c =>
                {
                    c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2);
                })
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.ClearProviders();
                    configureLogging.AddProvider(new KLoggerProvider());
                });
            await builder.RunConsoleAsync();
        }
    }
}
