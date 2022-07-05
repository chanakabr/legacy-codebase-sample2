using System;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class PurchaseValidator
    {
        public static void Validate(this KalturaPurchase model)
        {
            // validate purchase token
            if (string.IsNullOrEmpty(model.Currency))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPurchase.currency");
        }
    }
}