using WebAPI.Exceptions;
using WebAPI.Models.Users.UserSessionProfile;

namespace WebAPI.ModelsValidators
{
    public static class UserSessionProfileValidator
    {
        private const int MAX_CONDITIONS = 10;

        internal static void ValidateForAdd(this KalturaUserSessionProfile model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (model.Expression == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            model.ValidateExpression();
        }

        internal static void ValidateForUpdate(this KalturaUserSessionProfile model)
        {
            if (model.Expression != null)
            {
                model.ValidateExpression();
            }
        }

        private static void ValidateExpression(this KalturaUserSessionProfile model)
        {
            model.Expression.Validate();

            var totalConditions = model.Expression.ConditionsSum();
            if (totalConditions == 0)
            {
                throw new BadRequestException(BadRequestException.MISSING_MANDATORY_ARGUMENT_IN_PROPERTY, "expression", "KalturaUserSessionCondition");
            }

            if (totalConditions > MAX_CONDITIONS)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "expression's conditions", MAX_CONDITIONS);
            }
        }
    }
}
