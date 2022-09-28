using ApiLogic.Api.Managers;
using ApiLogic.Api.Validators;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Catalog.CatalogManagement.Validators;
using ApiLogic.EPG;
using ApiLogic.Notification;
using ApiLogic.Repositories;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.GroupManagers;
using Core.Notification;
using DAL;
using EpgNotificationHandler.Configuration;
using GroupsCacheManager;
using IotGrpcClientWrapper;
using Microsoft.Extensions.DependencyInjection;
using NotificationHandlers.Common;
using Module = Core.Domains.Module;

namespace EpgNotificationHandler.Infrastructure
{
    public static class EpgNotificationHandlerExtensions
    {
        public static IServiceCollection AddEpgNotificationHandlerDependencies(this IServiceCollection serviceCollection)
            => serviceCollection
                // use overload with delegate to be able to replace implementation (ex: in unit tests)
                .AddSingleton<IEpgPartnerConfigurationManager>(serviceProvider => EpgPartnerConfigurationManager.Instance)
                .AddSingleton<IIotClient>(serviceProvider => IotGrpcClientWrapper.IotClient.Instance)
                .AddSingleton<IGeneralPartnerConfigManager, GeneralPartnerConfigManager>()
                .AddSingleton<IGeneralPartnerConfigRepository, ApiDAL>()
                .AddSingleton<IEpgNotificationConfiguration, EpgNotificationConfiguration>()
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
                .AddSingleton(DeviceFamilyRepository.Instance);
    }
}