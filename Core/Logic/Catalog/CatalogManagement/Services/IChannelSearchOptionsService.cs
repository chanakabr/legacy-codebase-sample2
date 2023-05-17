using ApiLogic.Catalog.CatalogManagement.Models;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IChannelSearchOptionsService
    {
        ChannelSearchOptionsResult ResolveKsqlChannelSearchOptions(ChannelSearchOptionsContext context);
    }
}