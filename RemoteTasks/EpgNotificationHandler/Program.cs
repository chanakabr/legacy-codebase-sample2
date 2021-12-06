using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Validators;
using ApiLogic.Catalog.CatalogManagement.Repositories;
using ApiLogic.Notification;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using Core.Domains;
using Core.GroupManagers;
using Core.GroupManagers.Adapters;
using Core.Notification;
using DAL;
using EpgNotificationHandler.Configuration;
using EventBus.RabbitMQ;
using GroupsCacheManager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationHandlers.Common;
using WebAPI.Filters;
using Module = Core.Domains.Module;

namespace EpgNotificationHandler
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureMappings()
                .ConfigureEventNotificationsConfig()
                .ConfigureEventBusConsumer()
                .ConfigureServices(services =>
                {
                    services
                        .AddScoped<IIotManager, IotManager>()
                        .AddSingleton<IEpgNotificationConfiguration, EpgNotificationConfiguration>()
                        .AddSingleton<INotificationDal, NotificationDal>()
                        .AddSingleton<ILayeredCache, LayeredCache>()
                        .AddSingleton<IDomainModule, Module>()
                        .AddSingleton<INotificationCache, NotificationCache>()
                        .AddSingleton<ICatalogManager, CatalogManager>()
                        .AddSingleton<IRegionManager, RegionManager>()
                        .AddSingleton<IRegionValidator, RegionValidator>()
                        .AddSingleton<ILabelDal, LabelDal>()
                        .AddSingleton<ILabelRepository, LabelRepository>()
                        .AddSingleton<IAssetStructMetaRepository, AssetStructMetaRepository>()
                        .AddSingleton<IGroupSettingsManager, GroupSettingsManagerAdapter>()
                        .AddSingleton<IGroupManager, GroupManager>()
                        .AddSingleton<IIotNotificationService, IotNotificationService>();
                });

            AppMetrics.Start();

            await builder.RunConsoleAsync();
        }
    }
}
