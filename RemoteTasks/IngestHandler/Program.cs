using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiLogic.EPG;
using ApiLogic.Notification.Managers;
using ApiObjects;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using Core.Metrics;
using DAL;
using EventBus.Abstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using FeatureFlag;
using GroupsCacheManager;
using IngestHandler;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Kafka;
using IngestHandler.Domain.IngestProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ott.Lib.FeatureToggle.IocExtensions;
using OTT.Lib.Kafka;
using Phx.Lib.Log;
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
                    services.AddSingleton<IProgramAssetCrudEventMapper, ProgramAssetCrudEventMapper>()
                        // catalog manager
                        .AddScoped<ICatalogManager, CatalogManager>()
                        .AddScoped<ILabelRepository, LabelRepository>()
                        .AddScoped<ILabelDal, LabelDal>()
                        .AddSingleton<ILayeredCache, LayeredCache>()
                        .AddScoped<IAssetStructValidator, AssetStructValidator>()
                        .AddScoped<IAssetStructMetaRepository, AssetStructMetaRepository>()
                        .AddScoped<IAssetStructRepository, AssetStructRepository>()
                        .AddScoped<IGroupSettingsManager, GroupSettingsManager>()
                        .AddScoped<IEpgV2PartnerConfigurationManager>(serviceProvider => EpgV2PartnerConfigurationManager.Instance)
                        .AddScoped<IGroupManager, GroupManager>()
                        .AddSingleton<ICatalogCache, CatalogCache>();
                    services.AddScoped<IEpgIngestMessaging>(provider => new EpgIngestMessaging(
                        provider.GetService<IEventBusPublisher>(),
                        new KLogger(nameof(EpgIngestMessaging))));
                    services.AddScoped<IProgramAssetCrudMessageService>(provider => new ProgramAssetCrudMessageService(
                        provider.GetService<IAssetManager>(),
                        provider.GetService<IEpgAssetManager>(),
                        provider.GetService<IProgramAssetCrudEventMapper>(),
                        provider.GetService<IKafkaProducerFactory>(),
                        provider.GetService<IKafkaContextProvider>(),
                        new KLogger(nameof(ProgramAssetCrudMessageService))));
                    services.AddScoped<IIngestFinalizer, IngestFinalizer>();

                    services.AddFeatureToggle(new ConfigurationBuilder().AddEnvironmentVariables().Build());
                    services.AddScoped<IFeatureFlagContext, FeatureFlagIngestContext>();
                    services.AddScoped<IPhoenixFeatureFlag, PhoenixFeatureFlag>();
                })
                .ConfigureEventBusConsumer(c => { c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2); })
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.ClearProviders();
                    configureLogging.AddProvider(new KLoggerProvider());
                });
            await builder.RunConsoleAsync();
        }
    }
}