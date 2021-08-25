using Core.Catalog;

namespace IngestHandler.Common.Infrastructure
{
    public interface ICatalogManagerAdapter
    {
        bool DoesGroupUsesTemplates(int groupId);
        
        CatalogGroupCache GetCatalogGroupCache(int groupId);
    }
}
