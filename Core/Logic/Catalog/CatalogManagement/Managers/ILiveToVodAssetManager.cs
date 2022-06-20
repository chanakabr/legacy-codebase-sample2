using ApiObjects.Response;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public interface ILiveToVodAssetManager
    {
        GenericResponse<LiveToVodAsset> AddLiveToVodAsset(long partnerId, LiveToVodAsset asset, long updaterId);
        GenericResponse<LiveToVodAsset> UpdateLiveToVodAsset(long partnerId, long assetId, LiveToVodAsset asset, long updaterId);
        GenericResponse<long?> GetMediaIdByEpgId(long epgId);
    }
}