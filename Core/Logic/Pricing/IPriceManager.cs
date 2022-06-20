using ApiObjects.Response;

namespace Core.Pricing
{
    public interface IPriceManager
    {
        GenericResponse<AssetFilePpv> AddAssetFilePPV(int groupId, AssetFilePpv assetFilePpv);
        GenericResponse<AssetFilePpv> UpdateAssetFilePPV(int groupId, AssetFilePpv request);
        Status DeleteAssetFilePPV(int groupId, long mediaFileId, long ppvModuleId);
        GenericListResponse<AssetFilePpv> GetAssetFilePPVList(int groupId, long assetId, long assetFileId);
    }
}