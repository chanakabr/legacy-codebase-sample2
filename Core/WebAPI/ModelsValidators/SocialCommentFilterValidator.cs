using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.Social;

namespace WebAPI.ModelsValidators
{
    public static class SocialCommentFilterValidator
    {
        internal static void validate(this KalturaSocialCommentFilter model)
        {
            if (model.AssetIdEqual == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSocialCommentFilter.assetIdEqual");
            }
            if (model.AssetTypeEqual == KalturaAssetType.recording)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialCommentFilter.assetTypeEqual");
            }
            if (model.AssetTypeEqual == KalturaAssetType.epg && model.SocialPlatformEqual != KalturaSocialPlatform.IN_APP)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSocialCommentFilter.assetTypeEqual, KalturaSocialCommentFilter.socialPlatformEqual");
            }
        }
    }
}
