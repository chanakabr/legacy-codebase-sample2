using ApiLogic.Api.Managers;
using ApiLogic.Api.Validators;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiLogic.EPG;
using ApiLogic.Repositories;
using CachingProvider.LayeredCache;
using CloudfrontInvalidatorGrpcClientWrapper;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.GroupManagers;
using Core.Notification;
using DAL;
using EventBus.Abstraction;
using GroupsCacheManager;
using IotGrpcClientWrapper;
using Microsoft.Extensions.DependencyInjection;
using Module = Core.Domains.Module;

namespace LineupNotificationHandler.Infrastructure
{
    public static class LineupNotificationHandlerExtensions
    {
        public static IServiceCollection AddLineupNotificationHandlerDependencies(this IServiceCollection serviceCollection)
            => serviceCollection
                // use overload with delegate to be able to replace implementation (ex: in unit tests)
                .AddLogging()
                .AddSingleton<IEpgPartnerConfigurationManager>(serviceProvider => EpgPartnerConfigurationManager.Instance)
                .AddSingleton<IIotClient>(serviceProvider => IotClient.Instance)
                .AddSingleton<IGeneralPartnerConfigManager, GeneralPartnerConfigManager>()
                .AddSingleton<IGeneralPartnerConfigRepository, ApiDAL>()
                .AddSingleton<INotificationDal, NotificationDal>()
                .AddSingleton<ILayeredCache, LayeredCache>()
                .AddSingleton<IDomainModule, Module>()
                .AddSingleton<INotificationCache, NotificationCache>()
                .AddSingleton<ICatalogCache, CatalogCache>()
                .AddSingleton<ICatalogManager, CatalogManager>()
                .AddSingleton<IRegionManager, RegionManager>()
                .AddSingleton<IRegionValidator, RegionValidator>()
                .AddSingleton<ILabelDal, LabelDal>()
                .AddSingleton<ILabelRepository, LabelRepository>()
                .AddSingleton<IAssetStructValidator, AssetStructValidator>()
                .AddSingleton<IAssetStructMetaRepository, AssetStructMetaRepository>()
                .AddSingleton<IAssetStructRepository, AssetStructRepository>()
                .AddSingleton<IGroupSettingsManager, GroupSettingsManager>()
                .AddSingleton<IGroupManager, GroupManager>()
                .AddSingleton(DeviceFamilyRepository.Instance)
                .AddPhoenixFeatureFlag()
                .AddCloudfrontInvalidator()
            ;
    }
}
