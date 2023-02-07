using System;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Managers;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiLogic.EPG;
using ApiLogic.Pricing.Handlers;
using ApiLogic.Repositories;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using Core.Pricing;
using Core.Users.Cache;
using CouchbaseManager;
using DAL;
using DAL.MongoDB;
using ElasticSearch.Utilities;
using GroupsCacheManager;
using LiveToVod;
using LiveToVod.BOL;
using LiveToVod.DAL;
using log4net.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Catalog;
using Phoenix.AsyncHandler.ConditionalAccess;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.AsyncHandler.Kronos;
using Phoenix.AsyncHandler.Pricing;
using Phoenix.AsyncHandler.Recording;
using Phoenix.Generated.Api.Events.Crud.Household;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using Phoenix.Generated.Api.Events.Logical.appstoreNotification;
using Phoenix.Generated.Api.Events.Logical.IndexRecording;
using Phoenix.Generated.Api.Events.Logical.RebuildRecordingsIndex;
using Phoenix.Generated.Tasks.Recurring.EpgV3Cleanup;
using Phoenix.Generated.Tasks.Recurring.LiveToVodTearDown;
using Phoenix.Generated.Tasks.Recurring.ScheduleRecordingEvictions;
using Phoenix.Generated.Tasks.Scheduled.EvictRecording;
using Phoenix.Generated.Tasks.Scheduled.renewSubscription;
using Phoenix.Generated.Tasks.Scheduled.RetryRecording;
using Phoenix.Generated.Tasks.Scheduled.VerifyRecordingFinalStatus;
using Phx.Lib.Appconfig;
using Phx.Lib.Couchbase.IoC;
using Phx.Lib.Log;
using WebAPI.Filters;
using Module = Core.Pricing.Module;

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
                }).ConfigureMappings()
                .ConfigureEventNotificationsConfig();

            return builder;
        }

        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            var kafkaConfig = KafkaConfig.Get();
            services.AddSingleton(p => DomainsCache.Instance());
            services.AddScoped<IKafkaContextProvider, AsyncHandlerKafkaContextProvider>();

            services.AddKronosHandlers(kafkaConfig[KafkaConfigKeys.BootstrapServers],
                p => p
                    .AddHandler<ScheduleRecordingEvictionsHandler>(ScheduleRecordingEvictions.ScheduleRecordingEvictionsQualifiedName)
                    .AddHandler<RenewHandler>(RenewSubscription.RenewSubscriptionQualifiedName)
                    .AddHandler<LiveToVodTearDownHandler>(LiveToVodTearDown.LiveToVodTearDownQualifiedName)
                    .AddHandler<EpgV3CleanupHandler>(EpgV3Cleanup.EpgV3CleanupQualifiedName)
                    .AddHandler<VerifyRecordingFinalStatusHandler>(VerifyRecordingFinalStatus.VerifyRecordingFinalStatusQualifiedName)
                    .AddHandler<RetryRecordingHandler>(RetryRecording.RetryRecordingQualifiedName)
                    .AddHandler<EvictRecordingHandler>(EvictRecording.EvictRecordingQualifiedName)
                );

            services.AddKafkaProducerFactory(kafkaConfig);
            services
                .AddSingleton(p => DomainsCache.Instance())
                .AddScoped<ILiveToVodAssetManager, LiveToVodAssetManager>()
                .AddScoped<ILiveToVodAssetRepository, LiveToVodAssetRepository>()
                .AddScoped<ILiveToVodAssetCrudMessagePublisher, LiveToVodAssetCrudMessagePublisher>()
                .AddScoped<ILiveToVodImageService, LiveToVodImageService>()
                .AddScoped<IImageManager, Core.Catalog.CatalogManagement.ImageManager>()
                .AddScoped<ITtlService, TtlService>()
                .AddScoped<ILiveToVodAssetFileService, LiveToVodAssetFileService>()
                .AddScoped<IMediaFileTypeManager, FileManager>()
                .AddScoped<IPriceManager, PriceManager>()
                .AddScoped<ILiveToVodPpvModuleParser, LiveToVodPpvModuleParser>()
                .AddScoped<IPpvManager, PpvManager>()
                .AddScoped<IPpvManagerRepository, PricingDAL>()
                .AddScoped<IPriceDetailsManager, PriceDetailsManager>()
                .AddScoped<IPriceDetailsRepository, PricingDAL>()
                .AddScoped<IGeneralPartnerConfigManager, GeneralPartnerConfigManager>()
                .AddScoped<IGeneralPartnerConfigRepository, ApiDAL>()
                .AddScoped<IDeviceFamilyRepository, DeviceFamilyRepository>()
                .AddScoped<IDeviceFamilyDal, DeviceFamilyDal>()
                .AddScoped<ICountryManager, api>()
                .AddScoped<IDiscountDetailsManager, DiscountDetailsManager>()
                .AddScoped<IDiscountDetailsRepository, PricingDAL>()
                .AddScoped<IUsageModuleManager, UsageModuleManager>()
                .AddScoped<IModuleManagerRepository, PricingDAL>()
                .AddScoped<IModuleManagerRepository, PricingDAL>()
                .AddScoped<IPricingModule, Module>()
                .AddScoped<IVirtualAssetManager, api>()
                .AddScoped<IPriceDetailsRepository, PricingDAL>()
                .AddScoped<IProgramAssetCrudEventMapper, ProgramAssetCrudEventMapper>()
                .AddScoped<IAssetManager, AssetManager>()
                .AddScoped<IIndexManagerFactory, IndexManagerFactory>()
                .AddScoped<ILiveToVodManager, LiveToVodManager>()
                .AddScoped<ILiveToVodService, LiveToVodService>()
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
                .AddSingleton<ICatalogCache, CatalogCache>()
                //.AddSingleton<ICouchbaseWorker, CouchbaseWorker>()
                // live to vod
                .AddScoped<IConnectionStringHelper, TcmConnectionStringHelper>()
                .AddScoped<IClientFactoryBuilder, ClientFactoryBuilder>()
                .AddScoped<IRepository>(provider => new Repository(
                    provider.GetService<IClientFactoryBuilder>().GetClientFactory(DatabaseProperties.DATABASE),
                    provider.GetService<IClientFactoryBuilder>().GetAdminClientFactory(DatabaseProperties.DATABASE)));

            return services;
        }

        public static IServiceCollection AddKafkaHandlersFromAssembly(this IServiceCollection services)
        {
            services.AddKafkaHandler<IndexRecordingHandler, IndexRecording>("Index-Recording", IndexRecording.GetTopic());
            services.AddKafkaHandler<RebuildRecordingsIndexHandler, RebuildRecordingsIndex>("rebuild-recordings-index", RebuildRecordingsIndex.GetTopic());
            services.AddKafkaHandler<HouseholdNpvrAccountHandler, Household>("household-npvr-account", Household.GetTopic());
            services.AddKafkaHandler<EntitlementLogicalHandler, AppstoreNotification>("appstore-notification", AppstoreNotification.GetTopic());
            services.AddKafkaHandler<LiveToVodAssetHandler, ProgramAsset>("live-to-vod-asset", ProgramAsset.GetTopic());
            return services;
        }
        
        public static IServiceCollection AddCouchbase(this IServiceCollection services)
        {
            services.AddCouchbase();
            services.AddCouchbaseClient<IScheduledTasks>(eCouchbaseBucket.SCHEDULED_TASKS.ToString());
            return services;
        }

        private static IServiceCollection AddKronosHandlers(this IServiceCollection services,
            string brokerConnectionString,
            Action<KronosConfigurationProvider> configure)
        {
            var provider = new KronosConfigurationProvider();
            configure(provider);
            services.AddSingleton<TaskHandler.TaskHandlerBase, KronosTaskHandler>(
                p => new KronosTaskHandler(p.GetService<IServiceScopeFactory>(), provider));

            foreach (var (taskName, type) in provider.Handlers)
            {
                services
                    .AddScoped(type)
                    .AddSingleton<IHostedService>(p =>
                    {
                        // we do not use AddAsyncGrpcTaskHandlerService because it will call addHostedService which will register only a single hosted service
                        // see: https://github.com/dotnet/runtime/issues/38751
                        // as a special case we allow phoenix.asyncHandler to hold multiple task handlers, so we will register the hosted services manually like this.
                        TaskHandler.TaskHandlerBase requiredService = p.GetRequiredService<TaskHandler.TaskHandlerBase>();
                        IHostApplicationLifetime service = p.GetService<IHostApplicationLifetime>();
                        ILoggerFactory service2 = p.GetService<ILoggerFactory>();
                        return new AsyncGrpcTaskHandlerServerHostedService(taskName, requiredService, brokerConnectionString, service, service2);
                    });          
            }

            return services;
        }

        private static IServiceCollection AddKafkaHandler<THandler, TValue>(this IServiceCollection services, string kafkaGroupSuffix, string topic) where THandler : IHandler<TValue>
        {
            services.AddScoped(typeof(THandler), typeof(THandler));
            services.AddSingleton<IHostedService, BackgroundServiceStarter<THandler, TValue>>(p =>
                new BackgroundServiceStarter<THandler, TValue>(
                    p.GetService<IKafkaConsumerFactory>(),
                    p.GetService<IServiceScopeFactory>(),
                    p.GetService<IHostApplicationLifetime>(),
                    kafkaGroupSuffix,
                    topic));
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