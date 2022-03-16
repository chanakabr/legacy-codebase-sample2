using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class CollectionFilterValidator
    {
        public static void Validate(this KalturaCollectionFilter model)
        {
            if (model.MediaFileIdEqual.HasValue && !string.IsNullOrEmpty(model.CollectionIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaCollectionFilter.collectionIdIn", "KalturaCollectionFilter.mediaFileIdEqual");
            }
        }
    }
}