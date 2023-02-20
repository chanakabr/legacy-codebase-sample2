using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class SubscriptionSetFilterValidator
    {
        public static void Validate(this KalturaSubscriptionSetFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn) && !string.IsNullOrEmpty(model.SubscriptionIdContains))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionSetFilter.idIn, KalturaSubscriptionSetFilter.subscriptionIdContains");
            }
        }
    }
}
