using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.ModelsValidators
{
    public static class HouseholdCouponFilterValidator
    {
        public static void Validate(this KalturaHouseholdCouponFilter model)
        {
            if (model.BusinessModuleIdEqual == 0 && string.IsNullOrEmpty(model.CouponCode) && model.Status == null)
            {
                var filterName = "KalturaHouseholdCouponFilter";
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, $"{filterName}.businessModuleIdEqual, {filterName}.couponCode, {filterName}.status");
            }
        }
    }
}
