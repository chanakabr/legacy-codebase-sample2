using WebAPI.Exceptions;
using WebAPI.Models.MultiRequest;

namespace WebAPI.ModelsValidators
{
    public static class SkipConditionValidator
    {
        public static void Validate(this KalturaSkipCondition model)
        {
            switch (model)
            {
                //case KalturaAggregatedPropertySkipCondition:
                case KalturaPropertySkipCondition c: c.Validate(); break;
            }
        }

        private static void Validate(this KalturaPropertySkipCondition model)
        {
            if (string.IsNullOrEmpty(model.PropertyPath))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, model.objectType + ".propertyPath");
            }

            if (string.IsNullOrEmpty(model.Value))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, model.objectType + ".value");
            }
        }
    }
}
