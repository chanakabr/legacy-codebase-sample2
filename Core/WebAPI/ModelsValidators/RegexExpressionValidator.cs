using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.ModelsValidators
{
    public static class RegexExpressionValidator
    {
        public static void Validate(this KalturaRegexExpression model)
        {
            if (string.IsNullOrEmpty(model.Expression))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            if (string.IsNullOrEmpty(model.Description))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (!StringUtils.IsValidRegex(model.Expression))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "expression");
            }
        }
    }
}
