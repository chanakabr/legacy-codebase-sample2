using System.Collections.Generic;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILiveToVodAssetCrudMessagePublisher
    {
        void Publish(long partnerId, LiveToVodAsset asset, IEnumerable<string> files, int operationType);
    }
}