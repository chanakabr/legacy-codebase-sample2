using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class CompensationValidator
    {
        public static void Validate(this KalturaCompensation model)
        {
            if (model.CompensationType == KalturaCompensationType.PERCENTAGE && model.Amount > 100)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_VALUE_CROSSED,
                    "KalturaCompensation.amount", 100);
            }
        }
    }
}