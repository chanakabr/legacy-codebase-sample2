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
                if (model.MediaFileIdEqual.HasValue &&
                    (!string.IsNullOrEmpty(model.SubscriptionIdIn) || !string.IsNullOrEmpty(model.ExternalIdIn) || model.AlsoInactive.HasValue ||
                    model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue ||
                    !string.IsNullOrEmpty(model.NameContains)))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.MediaFileIdEqual", "KalturaSubscriptionFilter");

                if (!string.IsNullOrEmpty(model.SubscriptionIdIn) &&
                        (model.MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(model.ExternalIdIn) ||
                        model.AlsoInactive.HasValue || model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue ||
                        !string.IsNullOrEmpty(model.NameContains)))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.SubscriptionIdIn", "KalturaSubscriptionFilter");

                if (!string.IsNullOrEmpty(model.ExternalIdIn) &&
                        (model.MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(model.SubscriptionIdIn) || model.AlsoInactive.HasValue ||
                        model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue ||
                        !string.IsNullOrEmpty(model.NameContains)))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.productCodeIn", "KalturaSubscriptionFilter");

                if (!string.IsNullOrEmpty(model.NameContains) &&
                        (model.MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(model.SubscriptionIdIn) ||
                        model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue ||
                        !string.IsNullOrEmpty(model.ExternalIdIn)))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.nameContains", "KalturaSubscriptionFilter");

                if (model.CouponGroupIdEqual.HasValue && (model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.CouponGroupIdEqual", "KalturaSubscriptionFilter");
                if (model.PreviewModuleIdEqual.HasValue && (model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue))
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.PreviewModuleIdEqual", "KalturaSubscriptionFilter");
                if (model.PricePlanIdEqual.HasValue && model.ChannelIdEqual.HasValue)
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.model.PricePlanIdEqual", "KalturaSubscriptionFilter.model.ChannelIdEqual");
            }
            else
            {
                if (model.MediaFileIdEqual.HasValue || !string.IsNullOrEmpty(model.SubscriptionIdIn) || !string.IsNullOrEmpty(model.ExternalIdIn) || model.AlsoInactive.HasValue ||
                    model.PreviewModuleIdEqual.HasValue || model.PricePlanIdEqual.HasValue || model.ChannelIdEqual.HasValue)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSubscriptionFilter.Ksql", "KalturaSubscriptionFilter");
                }
            }

            if (!string.IsNullOrEmpty(model.SubscriptionIdIn))
            {
                Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.SubscriptionIdIn, "subscriptionIdIn", true);
            }
        }
    }
}