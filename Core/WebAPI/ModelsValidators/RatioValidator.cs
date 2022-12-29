using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class RatioValidator
    {
        public static void ValidateForAdd(this KalturaRatio model)
        {
            if (model.Height <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.height", 1);
            }

            if (model.Width <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.width", 1);
            }

            if (model.PrecisionPrecentage < 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "ratio.precisionPrecentage", 0);
            }

            if (model.PrecisionPrecentage > 100)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED, "ratio.precisionPrecentage", 100);
            }
        }
    }
}
