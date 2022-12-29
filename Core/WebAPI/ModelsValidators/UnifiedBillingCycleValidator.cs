using WebAPI.Exceptions;
using WebAPI.Models.Partner;

namespace WebAPI.ModelsValidators
{
    public static class UnifiedBillingCycleValidator
    {
        internal static void Validate(this KalturaUnifiedBillingCycle model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUnifiedBillingCycle.name");
            }

            if (model.Duration == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUnifiedBillingCycle.duration");
            }
        }
    }
}
