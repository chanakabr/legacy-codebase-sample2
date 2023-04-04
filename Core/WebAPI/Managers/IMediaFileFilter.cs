using System.Collections.Generic;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Managers
{
    public interface IMediaFileFilter
    {
        void FilterAssetFiles(KalturaAsset asset, int groupId, string sessionCharacteristicKey);
        void FilterAssetFiles(IEnumerable<KalturaAsset> assets, int groupId, string sessionCharacteristicKey);
        IEnumerable<KalturaPlaybackSource> GetFilteredAssetFiles(
            IEnumerable<KalturaPlaybackSource> mediaFiles,
            int groupId,
            long assetId,
            KalturaAssetType assetType,
            string sessionCharacteristicKey);
    }
}