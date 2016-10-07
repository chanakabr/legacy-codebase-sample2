using WebAPI.Mapping.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Filters
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            GeneralMeppings.RegisterMappings();
            UsersMappings.RegisterMappings();
            CatalogMappings.RegisterMappings();
            ApiMappings.RegisterMappings();            
            PricingMappings.RegisterMappings();
            ConditionalAccessMappings.RegisterMappings();
            DomainsMappings.RegisterMappings();
            BillingMappings.RegisterMappings();
            SocialMappings.RegisterMappings();
            PartnerMappings.RegisterMappings();
            NotificationMapping.RegisterMappings();
            DMSMapping.RegisterMappings();
        }
    }
}