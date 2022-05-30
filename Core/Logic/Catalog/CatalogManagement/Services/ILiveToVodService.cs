using ApiObjects.Catalog;
using ApiObjects.Response;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILiveToVodService
    {
        GenericResponse<AssetStruct> AddLiveToVodAssetStruct(int groupId, long userId);
        GenericResponse<AssetStruct> GetLiveToVodAssetStruct(int groupId);
    }
}