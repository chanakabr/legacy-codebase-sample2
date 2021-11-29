using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiObjects;
using Core.Metrics;
using EventBus.RabbitMQ;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Domain.IngestProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                })
                .ConfigureEventBusConsumer(c =>
                {
                    c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2);
                });
            await builder.RunConsoleAsync();
        }
    }
}
