using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class AssetStatisticsQueryValidator
    {
        internal static void Validate(this KalturaAssetStatisticsQuery model)
        {
            if (string.IsNullOrEmpty(model.AssetIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaAssetStatisticsQuery.assetIdIn");
            }

            if (model.AssetTypeEqual == KalturaAssetType.recording)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaAssetStatisticsQuery.assetTypeEqual", "KalturaAssetType.recording");
            }
        }
    }
}
