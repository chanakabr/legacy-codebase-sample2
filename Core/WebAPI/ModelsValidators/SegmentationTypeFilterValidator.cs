using System;
using WebAPI.Exceptions;
using WebAPI.Models.Segmentation;

namespace WebAPI.ModelsValidators
{
    public static class SegmentationTypeFilterValidator
    {
        public static bool Validate(this KalturaBaseSegmentationTypeFilter model)
        {
            switch (model)
            {
                case KalturaSegmentationTypeFilter c: return c.Validate(); 
                case KalturaSegmentValueFilter c: return c.Validate();
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }

        public static bool Validate(this KalturaSegmentationTypeFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn) && string.IsNullOrEmpty(model.Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            if (!string.IsNullOrEmpty(model.IdIn) && !string.IsNullOrEmpty(model.Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSegmentationTypeFilter.IdIn", "KalturaSegmentationTypeFilter.Ksql");
            }

            return true;
        }

        public static bool Validate(this KalturaSegmentValueFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            return true;
        }
    }
}
