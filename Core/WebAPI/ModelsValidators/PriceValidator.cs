using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class PriceValidator
    {
        public static void Validate(this KalturaPrice model)
        {
            switch (model)
            {
                case KalturaDiscount c: c.Validate(); break;
                default:
                {
                    if (!model.Amount.HasValue)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "amount");
                    }

                    if (model.Amount.Value < 0.01)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "amount", "0.01");
                    }

                    if (string.IsNullOrWhiteSpace(model.Currency))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "currency");
                    }
                    break;
                }
            }
        }
    }
}