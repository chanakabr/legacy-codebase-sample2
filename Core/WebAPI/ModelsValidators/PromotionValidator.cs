using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class PromotionValidator
    {
        public static void Validate(this KalturaPromotion model)
        {
            if (model.Conditions == null || model.Conditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "conditions");
            }

            foreach (var condition in model.Conditions)
            {
                condition.Validate();
            }
        }
    }
}