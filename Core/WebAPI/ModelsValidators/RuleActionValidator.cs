using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.ConditionalAccess.FilterActions.Assets;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ModelsValidators
{
    public static class RuleActionValidator
    {
        public static void Validate(this KalturaRuleAction model)
        {
            switch (model)
            {
                case KalturaFilterAssetByKsqlAction action:
                    action.Validate();
                    break;
            }
        }

        public static void Validate(this KalturaFilterAssetByKsqlAction model)
        {
            if (string.IsNullOrWhiteSpace(model.Ksql))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ksql");
            }
        }
    }
}