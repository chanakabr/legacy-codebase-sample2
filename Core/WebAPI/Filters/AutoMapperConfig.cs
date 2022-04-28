using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.Hosting;
using ObjectsConvertor.Mapping;
using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Filters
{
    public static class AutoMapperConfig
    {
        public static IHostBuilder ConfigureMappings(this IHostBuilder builder)
        {
            builder.ConfigureServices((hostContext, services) =>
            {
                RegisterMappings();
            });

            return builder;
        }

        public static void RegisterMappings()
        {
            MapperConfigurationExpression cfg = new MapperConfigurationExpression();
            GeneralMappings.RegisterMappings(cfg);
            UsersMappings.RegisterMappings(cfg);
            CatalogMappings.RegisterMappings(cfg);
            ApiMappings.RegisterMappings(cfg);
            PricingMappings.RegisterMappings(cfg);
            ConditionalAccessMappings.RegisterMappings(cfg);
            DomainsMappings.RegisterMappings(cfg);
            BillingMappings.RegisterMappings(cfg);
            SocialMappings.RegisterMappings(cfg);
            PartnerMappings.RegisterMappings(cfg);
            NotificationMapping.RegisterMappings(cfg);
            DMSMapping.RegisterMappings(cfg);
            SegmentationMapings.RegisterMappings(cfg);
            CanaryDeploymentMapping.RegisterMappings(cfg);
    
            Mapper.Initialize(cfg);
        }
    }
}