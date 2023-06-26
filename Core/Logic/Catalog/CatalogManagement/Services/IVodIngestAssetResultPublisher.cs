using ApiLogic.Catalog.CatalogManagement.Models;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IVodIngestAssetResultPublisher
    {
        void PublishFailedIngest(int groupId, ApiObjects.Response.Status status, string fileName);

        void Publish(VodIngestPublishContext context);
    }
}