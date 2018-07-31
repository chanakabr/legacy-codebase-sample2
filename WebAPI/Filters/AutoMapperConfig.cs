using AutoMapper;
using AutoMapper.Configuration;
using ObjectsConvertor.Mapping;
using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Filters
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            MapperConfigurationExpression cfg = new MapperConfigurationExpression();
            GeneralMeppings.RegisterMappings(cfg);
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

            Mapper.Initialize(cfg);

        }
    }
}