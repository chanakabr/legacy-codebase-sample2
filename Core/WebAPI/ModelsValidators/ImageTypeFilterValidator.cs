using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class ImageTypeFilterValidator
    {
        public static void Validate(this KalturaImageTypeFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn) && !string.IsNullOrEmpty(model.RatioIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageTypeFilter.idIn", "KalturaImageTypeFilter.ratioIdIn");
            }
        }
    }
}