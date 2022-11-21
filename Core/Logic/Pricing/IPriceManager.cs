using ApiObjects.Base;
using ApiObjects.Response;

namespace Core.Pricing
{
    public interface IPriceManager
    {
        GenericResponse<AssetFilePpv> AddAssetFilePPV(ContextData contextData, AssetFilePpv assetFilePpv);
        GenericResponse<AssetFilePpv> UpdateAssetFilePPV(ContextData contextData, AssetFilePpv request);
        Status DeleteAssetFilePPV(ContextData contextData, long mediaFileId, long ppvModuleId);
        GenericListResponse<AssetFilePpv> GetAssetFilePPVList(ContextData contextData, long assetId, long assetFileId);
    }
}