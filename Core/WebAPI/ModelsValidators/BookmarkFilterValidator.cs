using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class BookmarkFilterValidator
    {
        internal static void Validate(this KalturaBookmarkFilter model)
        {
            if (model.AssetIn != null && model.AssetIn.Count > 0)
            {
                return;
            }

            if (string.IsNullOrEmpty(model.AssetIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaBookmarkFilter.assetIdIn");
            }

            if (!model.AssetTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaBookmarkFilter.assetTypeEqual");
            }
        }
    }
}
