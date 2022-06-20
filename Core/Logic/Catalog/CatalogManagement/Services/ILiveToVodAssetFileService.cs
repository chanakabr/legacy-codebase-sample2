using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILiveToVodAssetFileService
    {
        IEnumerable<AssetFile> AddAssetFiles(long partnerId, long assetId, IEnumerable<AssetFile> filesToAdd, long updaterId);
        IEnumerable<AssetFile> UpdateAssetFiles(long partnerId, IEnumerable<AssetFile> assetFiles, MediaAsset asset, long updaterId);
        void AssignPpvOnAssetUpdated(
            long partnerId,
            long assetId,
            IEnumerable<AssetFile> assetFiles,
            IEnumerable<PpvModuleInfo> ppv);
        void AssignPpvOnAssetCreated(long partnerId, long assetId, IEnumerable<AssetFile> assetFiles, IEnumerable<PpvModuleInfo> ppv);
    }
}