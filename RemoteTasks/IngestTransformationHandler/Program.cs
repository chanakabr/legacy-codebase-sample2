using System.Threading.Tasks;
using ApiLogic.IndexManager.Mappings;
using ApiObjects;
using Core.Metrics;
using EventBus.RabbitMQ;
using IngestHandler.Common.Infrastructure;
using IngestTransformationHandler.Managers;
using IngestTransformationHandler.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                .ConfigureServices(s =>
                {
                    s.AddScoped<IEpgRepository, EpgRepository>();
                    s.AddScoped<IEpgCRUDOperationsManager, EpgCRUDOperationsManager>();
                    s.AddScoped<IMappingTypeResolver, MappingTypeResolver>();
                    s.AddSingleton<ICatalogManagerAdapter, CatalogManagerAdapter>();
                    s.AddSingleton<IndexCompactionManager, IndexCompactionManager>();
                })
                .ConfigureEventBustConsumer(c =>
                {
                    c.DedicatedPartnerIdsResolver = () => GroupsFeatures.GetGroupsThatImplementFeature(GroupFeature.EPG_INGEST_V2);
                });
            await builder.RunConsoleAsync();
        }
    }
}