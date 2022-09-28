using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Mappings;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Pricing;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using Core.Metrics;
using CouchbaseManager;
using DAL;
using EventBus.Abstraction;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using FeatureFlag;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Kafka;
using IngestTransformationHandler;
using IngestHandler.Common.Managers;
using IngestHandler.Common.Repositories;
using Microsoft.Extensions.Configuration;
using Phx.Lib.Log;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Ott.Lib.FeatureToggle.IocExtensions;
using OTT.Lib.Kafka;
using OTT.Lib.MongoDB;
using Tvinci.Core.DAL;
using TvinciCache;
using WebAPI.Filters;
using Microsoft.AspNetCore.Http;
using System.Threading;
using Core.Api;

namespace IngestHandler
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
                    s.AddMongoDbClientFactory(EpgMongoDB.Configuration, EpgMongoDB.DB_NAME);
                    s.AddScoped<IEpgRepository, EpgRepository>();
                    s.AddScoped<IEpgCRUDOperationsManager, EpgCRUDOperationsManager>();
                    s.AddScoped<IMappingTypeResolver, MappingTypeResolver>();
                    s.AddSingleton<ICatalogManagerAdapter, CatalogManagerAdapter>();
                    s.AddSingleton<IndexCompactionManager, IndexCompactionManager>();
                    s.AddSingleton<IEpgDal, EpgDal>();
                    s.AddSingleton<IChannelRepository>(provider => CatalogDAL.Instance);
                    s.AddSingleton<IIngestProfileRepository, IngestProfileRepository>();
                    s.AddSingleton<IXmlTvDeserializer, XmlTvDeserializer>();
                    s.AddSingleton<IGroupSettingsManager, GroupSettingsManager>();
                    s.AddSingleton<ILayeredCache, LayeredCache>();
                    s.AddSingleton<IIngestStagingRepository, IngestStagingRepository>();
                    s.AddSingleton<IEpgPartnerConfigurationManager>(provider => EpgPartnerConfigurationManager.Instance);

                    s.AddScoped<IngestV2TransformationHandler>();
                    s.AddScoped<IngestV3TransformationHandler>();


                    s.AddKafkaProducerFactory(KafkaConfig.Get());
                    s.AddKafkaContextProvider<EventBusKafkaContextProvider>();
                    s.AddScoped<IEventBusPublisher, KafkaPublisher>();
                    s.AddScoped<IEpgIngestMessaging>(provider => new EpgIngestMessaging(provider.GetService<IEventBusPublisher>(), new KLogger(nameof(EpgIngestMessaging))));

                    s.AddFeatureToggle(new ConfigurationBuilder().AddEnvironmentVariables().Build());
                    s.AddScoped<IFeatureFlagContext, FeatureFlagIngestContext>();
                    s.AddScoped<IPhoenixFeatureFlag, PhoenixFeatureFlag>();
                })
                .ConfigureEventBusConsumer(c => { c.DedicatedPartnerIdsResolver = EpgIngestPartnerResolver.GetRelevantPartnerIds; })
                .ConfigureLogging(configureLogging =>
                {
                    configureLogging.ClearProviders();
                    configureLogging.AddProvider(new KLoggerProvider());
                });
            await builder.RunConsoleAsync();
        }
    }
}