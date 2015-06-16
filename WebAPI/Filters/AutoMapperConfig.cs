using WebAPI.Mapping.ObjectsConvertor;

namespace WebAPI.Exceptions
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            UsersMappings.RegisterMappings();
            CatalogMappings.RegisterMappings();
            ApiMappings.RegisterMappings();            
            PricingMappings.RegisterMappings();
            ConditionalAccessMappings.RegisterMappings();
            DomainsMappings.RegisterMappings();
        }
    }
}