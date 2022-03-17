using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class KalturaImageFilterValidator
    {
        public static void Validate(this KalturaImageFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                if (model.ImageObjectIdEqual.HasValue && model.ImageObjectIdEqual != 0 && model.ImageObjectTypeEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageFilter.idIn", "KalturaImageFilter.model.ImageObjectIdEqual");
                }
                else if (!string.IsNullOrEmpty(model.ImageObjectIdIn) && model.ImageObjectTypeEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaImageFilter.model.ImageObjectIdIn", "KalturaImageFilter.model.ImageObjectIdEqual");
                }
            }

            if (model.ImageObjectIdEqual.HasValue && model.ImageObjectIdEqual != 0 && !model.ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.model.ImageObjectTypeEqual");
            }

            if (model.ImageObjectTypeEqual.HasValue)
            {
                if ((!model.ImageObjectIdEqual.HasValue || model.ImageObjectIdEqual == 0) && string.IsNullOrEmpty(model.ImageObjectIdIn))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{"KalturaImageFilter.model.ImageObjectIdEqual, KalturaImageFilter.model.ImageObjectIdIn"}");
                }
            }

            if (!model.ImageObjectIdEqual.HasValue && !model.ImageObjectTypeEqual.HasValue && string.IsNullOrEmpty(model.IdIn) && string.IsNullOrEmpty(model.ImageObjectIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{"KalturaImageFilter.model.ImageObjectIdEqual, KalturaImageFilter.idIn, KalturaImageFilter.model.ImageObjectIdIn"}");
            }

            if (!string.IsNullOrEmpty(model.ImageObjectIdIn) && !model.ImageObjectTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaImageFilter.model.ImageObjectIdIn");
            }
        }
    }
}