using System;
using ApiObjects.Pricing;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class DiscountDetailsValidator
    {
        public static void ValidateForAdd(this KalturaDiscountDetails model)
        {
            if (string.IsNullOrWhiteSpace(model.name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");

            // If nothing send
            if (model.StartDate.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "startDate");

            // If nothing send
            if (model.EndtDate.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "EndtDate");

            if (model.MultiCurrencyDiscount.Count.Equals(0))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multiCurrencyDiscount");

            if (model.WhenAlgoType != 0)
            {
                if (!Enum.IsDefined(typeof(WhenAlgoType), model.WhenAlgoType))
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "WhenAlgoType", model.WhenAlgoType);
            } 
            else
            {
                model.WhenAlgoType = (int) ApiObjects.Pricing.WhenAlgoType.N_FIRST_TIMES;
            }

            model.ValidateMultiCurrencyDiscount();
        }

       public static void ValidateForUpdate(this KalturaDiscountDetails model)
        {
            if(model.MultiCurrencyDiscount != null)
            {
                if (model.MultiCurrencyDiscount.Count == 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multiCurrencyDiscount");

                model.ValidateMultiCurrencyDiscount();
            }
        }

       public static void ValidateMultiCurrencyDiscount(this KalturaDiscountDetails model)
       {
            foreach (KalturaDiscount discount in model.MultiCurrencyDiscount)
            {
                discount.Validate();
            }
        }
    }
}