using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class AssetFilePpvFilterValidator
    {
        public static void Validate(this KalturaAssetFilePpvFilter model)
        {
            if (!model.AssetFileIdEqual.HasValue && !model.AssetIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "AssetFileIdEqual, AssetIdEqual");
            }

            if (model.AssetFileIdEqual.HasValue && model.AssetIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetFilePpvFilter.AssetFileIdEqual", "KalturaAssetFilePpvFilter.AssetIdEqual");
            }
        }
    }
}
