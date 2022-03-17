using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SubscriptionFilterMapper
    {
        public static List<long> getSubscriptionIdIn(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.SubscriptionIdIn))
                return null;

            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.SubscriptionIdIn, "subscriptionIdIn", true);
        }

        public static List<string> getExternalIdIn(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.ExternalIdIn))
                return null;

            return model.ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}