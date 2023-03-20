using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Mappings;
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
using Ingesthandler.common.Generated.Api.Events.ChannelIngestStaged;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Kafka;
using IngestHandler.Common.Managers;
using IngestHandler.Common.Repositories;
using IngestHandler.Domain.IngestProtection;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ott.Lib.FeatureToggle.IocExtensions;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;
using Tvinci.Core.DAL;
using TvinciCache;
using WebAPI.Filters;
using System;
using Ingesthandler.common.Generated.Api.Events.UpdateBulkUpload;
using Core.Api;
using System.Collections.Generic;
using System.Linq;
using IngestHandler.Common.Managers.Abstractions;

namespace EPGTransformationHandler
{
    internal static class Program
    {

        private const string CONSUMER_GROUP = "epg.ingestHandler.consumers";

        public static async Task Main(string[] args)
        {
            Metrics.CollectDefaultAndStartServer();
            AutoMapperConfig.RegisterMappings();

            var ingestV2Host = new HostBuilder()
                .ConfigureEventNotificationsConfig()
                .ConfigureCommonIngestServices()
                .ConfigureServices(s =>
                {
                    s.AddKafkaContextProvider<EventBusKafkaContextProvider>();
                    s.AddKafkaProducerFactory(KafkaConfig.Get());
                })
                .ConfigureEventBusConsumer(c =>
                    c.DedicatedPartnerIdsResolver = EpgIngestPartnerResolver.GetRelevantPartnerIds
                )
                .ConfigureCommonHostLogging();

            var IngestV3Host = new HostBuilder()
                .ConfigureEventNotificationsConfig()
                .ConfigureCommonIngestServices()
                .ConfigureServices(s =>
                {
                    s.AddKafkaConsumerFactory(KafkaConfig.Get());
                    s.AddKafkaProducerFactory(KafkaConfig.Get());
                    s.AddKafkaConsumerInterceptor<KafkaConsumerInterceptor>();
                    s.AddScoped<IEventContext, ManualKafkaContextProvider>();
                    s.AddKafkaContextProvider<ManualKafkaContextProvider>();
                })
                .AddKafkaConsumer<IngestV3Handler, ChannelIngestStaged>(CONSUMER_GROUP, ChannelIngestStaged.GetTopic())
                .AddKafkaConsumer<BulkUploadUpdater, UpdateBulkUpload>(CONSUMER_GROUP, UpdateBulkUpload.GetTopic())
                .ConfigureCommonHostLogging();

            var ingestV2Task = ingestV2Host.RunConsoleAsync();
            var ingestV3Task = IngestV3Host.RunConsoleAsync();

            await Task.WhenAny(ingestV2Task, ingestV3Task);
        }


        private static IHostBuilder ConfigureCommonHostLogging(this IHostBuilder host)
        {
            // TODO: use proper function to map log levels of ILogger and KLogger
            var klogLvl = KLogger.GetLogLevel();
            var netCoreLogLevel = LogLevel.Information;
            if (klogLvl == Level.Error) netCoreLogLevel = LogLevel.Error;
            if (klogLvl == Level.Info) netCoreLogLevel = LogLevel.Information;
            if (klogLvl == Level.Debug) netCoreLogLevel = LogLevel.Debug;
            if (klogLvl <= Level.Trace) netCoreLogLevel = LogLevel.Trace;

            return host.ConfigureLogging(configureLogging =>
            {
                configureLogging.ClearProviders();
                configureLogging.AddProvider(new KLoggerProvider());
                configureLogging.SetMinimumLevel(netCoreLogLevel);
            });
        }
        private static IHostBuilder ConfigureCommonIngestServices(this IHostBuilder host)
        {
            return host.ConfigureServices(s =>
            {
                s.AddMongoDbClientFactory(EpgMongoDB.Configuration, EpgMongoDB.DB_NAME);
                s.AddSingleton<IBulkUploadRepository, BulkUploadRepository>();
                s.AddSingleton<IBulkUploadService, BulkUploadService>();
                s.AddSingleton<IBulkCompletedRetryPolicyConfiguration, StaticBulkCompletedRetryPolicyConfiguration>();
                s.AddSingleton<IIngestStagingRepository, IngestStagingRepository>();
                s.AddSingleton<IEpgPartnerConfigurationManager>(_ => EpgPartnerConfigurationManager.Instance);
                s.AddSingleton<IEpgRepository, EpgRepository>();
                s.AddSingleton<IMappingTypeResolver>(_ => MappingTypeResolver.Instance);
                s.AddSingleton<IChannelRepository>(_ => CatalogDAL.Instance);
                s.AddSingleton<IIngestProfileRepository, IngestProfileRepository>();
                s.AddSingleton<IXmlTvDeserializer, XmlTvDeserializer>();
                s.AddSingleton<IEpgCRUDOperationsManager, EpgCRUDOperationsManager>();
                s.AddSingleton<IIngestProtectProcessor, IngestProtectProcessor>();
                s.AddSingleton<IEpgDal, EpgDal>();
                s.AddSingleton<ICatalogManagerAdapter, CatalogManagerAdapter>();
                s.AddSingleton(EpgAssetMultilingualMutator.Instance);
                s.AddSingleton<IRegionManager>(RegionManager.Instance);
                s.AddSingleton<IAssetManager>(AssetManager.Instance);
                s.AddSingleton<IEpgAssetManager>(EpgAssetManager.Instance);
                s.AddSingleton<IIndexManagerFactory>(IndexManagerFactory.Instance);
                s.AddSingleton(EpgNotificationManager.Instance());
                s.AddScoped<IEventBusPublisher, KafkaPublisher>();
                s.AddSingleton<IProgramAssetCrudEventMapper, ProgramAssetCrudEventMapper>()
                    // catalog manager
                    .AddScoped<ICatalogManager, CatalogManager>()
                    .AddScoped<ILabelRepository, LabelRepository>()
                    .AddScoped<ILabelDal, LabelDal>()
                    .AddSingleton<ILayeredCache, LayeredCache>()
                    .AddScoped<IAssetStructValidator, AssetStructValidator>()
                    .AddScoped<IAssetStructMetaRepository, AssetStructMetaRepository>()
                    .AddScoped<IAssetStructRepository, AssetStructRepository>()
                    .AddScoped<IGroupSettingsManager, GroupSettingsManager>()
                    .AddScoped<IEpgPartnerConfigurationManager>(serviceProvider => EpgPartnerConfigurationManager.Instance)
                    .AddScoped<IGroupManager, GroupManager>()
                    .AddSingleton<ICatalogCache, CatalogCache>();
                s.AddScoped<IEpgIngestMessaging>(p => new EpgIngestMessaging(
                    p.GetService<IEventBusPublisher>(),
                    new KLogger(nameof(EpgIngestMessaging))));
                s.AddScoped<IProgramAssetCrudMessageService>(p => new ProgramAssetCrudMessageService(
                    p.GetService<IAssetManager>(),
                    p.GetService<IEpgAssetManager>(),
                    p.GetService<IProgramAssetCrudEventMapper>(),
                    p.GetService<IKafkaProducerFactory>(),
                    p.GetService<IKafkaContextProvider>(),
                    new KLogger(nameof(ProgramAssetCrudMessageService))));
                s.AddScoped<IIngestFinalizer, IngestFinalizer>();
                s.AddPhoenixFeatureFlag();
            });
        }
    }
}
