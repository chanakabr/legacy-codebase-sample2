using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class SubscriptionFilterValidator
    {
        public static void Validate(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.Ksql))
            {
                if (model.CouponGroupIdEqual.HasValue && (model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.CouponGroupIdEqual", "KalturaSubscriptionFilter");
                if (model.PreviewModuleIdEqual.HasValue && (model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.PreviewModuleIdEqual", "KalturaSubscriptionFilter");
                if(model.PricePlanIdEqual.HasValue && model.ChannelIdEqual.HasValue)
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.PricePlanIdEqual", "KalturaSubscriptionFilter.model.ChannelIdEqual");
            }

            if (!string.IsNullOrEmpty(model.SubscriptionIdIn))
            {
                Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.SubscriptionIdIn, "subscriptionIdIn", true);
            }
        }
    }
}