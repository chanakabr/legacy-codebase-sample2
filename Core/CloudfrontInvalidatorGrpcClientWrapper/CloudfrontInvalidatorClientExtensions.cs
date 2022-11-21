using ApiObjects.Cloudfront;
using FeatureFlag;
using Phx.Lib.Appconfig;
using GrpcClientCommon;
using Microsoft.Extensions.DependencyInjection;
using OTT.Service.CloudfrontInvalidator;

namespace CloudfrontInvalidatorGrpcClientWrapper
{
    public static class CloudfrontInvalidatorExtensions
    {
        public static IServiceCollection AddCloudfrontInvalidator(this IServiceCollection services)
        {
            services.AddSingleton(p =>
                new CloudfrontInvalidator.CloudfrontInvalidatorClient(GrpcCommon.CreateChannel(ApplicationConfiguration
                    .Current.MicroservicesClientConfiguration.CloudfrontInvalidator)));
            services.AddScoped<CloudfrontForceInvalidator>();
            services.AddScoped<CloudfrontTtlInvalidator>();
            services.AddScoped(provider =>
                provider.GetRequiredService<IPhoenixFeatureFlag>().IsCloudfrontInvalidationEnabled()
                    ? (ICloudfrontInvalidator)provider.GetService<CloudfrontForceInvalidator>()
                    : provider.GetService<CloudfrontTtlInvalidator>());
            
            return services;
        }
    }
}