using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Notification.Managers;
using ApiObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.Metrics;
using EventBus.Abstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using IngestHandler;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Kafka;
using IngestHandler.Domain.IngestProtection;
using Phx.Lib.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Tvinci.Core.DAL;
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
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IIngestProtectProcessor, IngestProtectProcessor>();
                    services.AddSingleton<IEpgDal, EpgDal>();
                    services.AddSingleton<ICatalogManagerAdapter, CatalogManagerAdapter>();
                    services.AddSingleton(EpgAssetMultilingualMutator.Instance);
                    services.AddSingleton<IRegionManager>(RegionManager.Instance);
                    services.AddSingleton<IAssetManager>(AssetManager.Instance);
                    services.AddSingleton<IEpgAssetManager>(EpgAssetManager.Instance);
                    services.AddSingleton<IIndexManagerFactory>(IndexManagerFactory.Instance);
                    services.AddSingleton(EpgNotificationManager.Instance());
                    services.AddKafkaProducerFactory(KafkaConfig.Get());
                    services.AddKafkaContextProvider<IngestKafkaContextProvider>();
                    services.AddScoped<IEventBusPublisher, KafkaPublisher>();
                    services.AddScoped<IEpgIngestMessaging>(provider => new EpgIngestMessaging(
                        provider.GetService<IEventBusPublisher>(),
                        new KLogger(nameof(EpgIngestMessaging))));
                    services.AddScoped<IProgramAssetCrudMessageService>(provider => new ProgramAssetCrudMessageService(
                        provider.GetService<IAssetManager>(),
                        provider.GetService<IEpgAssetManager>(),
                        provider.GetService<IKafkaProducerFactory>(),
                        provider.GetService<IKafkaContextProvider>(),
                        new KLogger(nameof(ProgramAssetCrudMessageService))));
                    services.AddScoped<IIngestFinalizer, IngestFinalizer>();
                })
                .ConfigureEventBusConsumer(c => { c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2); })
                .ConfigureLogging(configureLogging => configureLogging.AddLog4Net());
            await builder.RunConsoleAsync();
        }
    }
}