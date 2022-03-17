using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.Social;

namespace WebAPI.ModelsValidators
{
    public static class SocialFriendActivityFilterValidator
    {
        public static void Validate(this KalturaSocialFriendActivityFilter model)
        {
            if (model.AssetIdEqual.HasValue && model.AssetIdEqual.Value > 0 && !model.AssetTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSocialFriendActivityFilter.assetTypeEqual");
            }

            if (model.AssetTypeEqual.HasValue && model.AssetTypeEqual.Value != KalturaAssetType.media)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialFriendActivityFilter.assetTypeEqual", model.AssetTypeEqual.Value);
            }
        }
    }
}