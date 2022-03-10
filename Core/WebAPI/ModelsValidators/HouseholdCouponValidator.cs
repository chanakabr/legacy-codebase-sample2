using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.ModelsValidators
{
    public static class HouseholdCouponValidator
    {
        public static void ValidateForAdd(this KalturaHouseholdCoupon model)
        {
            if (string.IsNullOrEmpty(model.Code))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
            }
        }
    }
}
