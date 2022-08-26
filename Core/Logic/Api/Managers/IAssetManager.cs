using System;
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
        IEnumerable<Asset> GetAssets(long groupId, IEnumerable<KeyValuePair<eAssetTypes, long>> assetTypes, bool isAllowedToViewInactiveAssets);
        GenericResponse<Asset> GetAsset(int groupId, long id, eAssetTypes assetType, bool isAllowedToViewInactiveAssets);
        Status DeleteAsset(int groupId, long id, eAssetTypes assetType, long userId, bool isFromChannel = false);
        GenericResponse<Asset> AddAsset(int groupId, Asset assetToAdd, long userId, bool isFromIngest = false);
        GenericResponse<Asset> UpdateAsset(int groupId, long id, Asset assetToUpdate, long userId, bool isFromIngest = false,
            bool isCleared = false, bool isForMigration = false, bool isFromChannel = false);
        void DeleteAssetsByTypeAndDate(long partnerId, long assetStructId, DateTime finalEndDate, long userId);
    }
}