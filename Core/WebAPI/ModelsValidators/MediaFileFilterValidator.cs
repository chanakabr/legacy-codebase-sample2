using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class MediaFileFilterValidator
    {
        internal static void Validate(this KalturaMediaFileFilter model)
        {
            if (model.AssetIdEqual > 0 && model.IdEqual > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMediaFileFilter.idEqual", "KalturaMediaFileFilter.assetIdEqual");
            }
        }
    }
}
