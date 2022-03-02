using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.ModelsValidators
{
    public static class PasswordPolicyValidator
    {
        public static void ValidateForAdd(this KalturaPasswordPolicy model)
        {
            if (string.IsNullOrEmpty(model.UserRoleIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userRoleIds");
            }
            ValidateComplexities(model.Complexities);
        }

        public static void ValidateForUpdate(this KalturaPasswordPolicy model)
        {
            ValidateComplexities(model.Complexities);
        }

        private static void ValidateComplexities(List<KalturaRegexExpression> complexities)
        {
            if (complexities?.Count > 0)
            {
                foreach (var pattern in complexities)
                {
                    pattern.Validate();
                }
            }
        }
    }
}
