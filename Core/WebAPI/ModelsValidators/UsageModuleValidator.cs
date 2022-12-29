using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class UsageModuleValidator
    {
        public static void ValidateForAdd(this KalturaUsageModule model)
        {
            if (string.IsNullOrEmpty(model.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            if (!model.FullLifeCycle.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fullLifeCycle");

            if (!model.ViewLifeCycle.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "viewLifeCycle");

            switch (model)
            {
                case KalturaPricePlan c: c.ValidateForAdd(); break;
            }
        }

        private static void ValidateForAdd(this KalturaPricePlan model)
        {
            if (!model.PriceDetailsId.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "priceDetailsId");
            if (!model.RenewalsNumber.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "renewalsNumber");
        }
    }
}
