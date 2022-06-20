using System.Collections.Generic;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILiveToVodImageService
    {
        void AddImages(long partnerId, IEnumerable<Image> imagesToAdd, long assetId, long updaterId);
        void UpdateImages(long partnerId, IEnumerable<Image> imagesToUpsert, long assetId, long updaterId);
    }
}