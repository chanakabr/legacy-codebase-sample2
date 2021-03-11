using ConfigurationManager;
using EpgCacheGrpcClientWrapper;
using GrpcClientCommon;
using Microsoft.Extensions.DependencyInjection;
using OTT.Service.EpgCache;
using Polly;

namespace EpgNotificationHandler.EpgCache
{
    public static class EpgCacheClientExtensions
    {
        public static IServiceCollection AddEpgCacheClient(this IServiceCollection services)
        {
            var epgCacheConfig = ApplicationConfiguration.Current.MicroservicesClientConfiguration.EpgCache;
            
            services.AddSingleton(p =>
                new Epgcache.EpgcacheClient(GrpcCommon.CreateChannel(epgCacheConfig.Address.Value,
                    epgCacheConfig.CertFilePath.Value)));
            // TODO https://anthonygiretti.com/2020/03/31/grpc-asp-net-core-3-1-resiliency-with-polly/
            services.AddScoped<IAsyncPolicy>(p => PolicyHandler.WaitAndRetry());
            services.AddScoped<IEpgCacheClient, EpgCacheClient>();
            
            return services;
        }
    }
}