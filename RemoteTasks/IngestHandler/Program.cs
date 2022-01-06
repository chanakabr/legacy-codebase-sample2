using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiObjects;
using Core.Catalog.CatalogManagement.Services;
using Core.Metrics;
using EventBus.Kafka;
using EventBus.RabbitMQ;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Domain.IngestProtection;
using Phx.Lib.Log;
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
                    services.AddScoped<IEpgIngestMessaging>(provider =>
                        new EpgIngestMessaging(KafkaPublisher.GetFromTcmConfiguration(),
                            new KLogger(nameof(EpgIngestMessaging))));
                })
                .ConfigureEventBusConsumer(c =>
                {
                    c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2);
                });
            await builder.RunConsoleAsync();
        }
    }
}
