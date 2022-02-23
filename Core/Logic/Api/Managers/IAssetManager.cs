using System.Collections.Generic;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;

namespace ApiLogic.Api.Managers
{
    public interface IAssetManager
    {
        bool InvalidateAsset(eAssetTypes assetType, int groupId, long assetId, [System.Runtime.CompilerServices.CallerMemberName] string callingMethod = "");
        GenericListResponse<Asset> GetLinearChannels(long groupId, IEnumerable<long> linearChannelIds, UserSearchContext searchContext);
        IEnumerable<Asset> GetAssets(long groupId, IEnumerable<KeyValuePair<eAssetTypes, long>> assetTypes, bool isAllowedToViewInactiveAssets);
    }
}