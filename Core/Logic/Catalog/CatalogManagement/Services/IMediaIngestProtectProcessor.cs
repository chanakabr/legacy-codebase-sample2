using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IMediaIngestProtectProcessor
    {
        void ProcessIngestProtect(MediaAsset oldAsset, MediaAsset newAsset, CatalogGroupCache catalogGroupCache);
    }
}