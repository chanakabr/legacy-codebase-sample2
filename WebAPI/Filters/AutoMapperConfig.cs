using WebAPI.ObjectsConvertor.Mapping;

namespace WebAPI.Filters
{
    public class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            UsersMappings.RegisterMappings();
            CatalogMappings.RegisterMappings();
            ApiMappings.RegisterMappings();
            ConditionalAccessMappings.RegisterMappings();
            DomainsMappings.RegisterMappings();
        }
    }
}