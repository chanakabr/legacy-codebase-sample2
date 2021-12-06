using APILogic.AdyenPayAPI;
using Core.Pricing;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class DiscountValidator
    {
        public static void Validate(this KalturaDiscount model)
        {
            if (string.IsNullOrWhiteSpace(model.Currency))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "currency");

            if (model.Amount > 0 && model.Percentage > 0)
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "amount", "Percentage");

            if (model.Amount == 0 && model.Percentage == 0)
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "amount, Percentage");
        }
    }
}