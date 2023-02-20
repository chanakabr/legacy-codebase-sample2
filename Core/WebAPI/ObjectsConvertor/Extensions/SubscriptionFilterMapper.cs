using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SubscriptionFilterMapper
    {
        public static HashSet<long> getSubscriptionIdIn(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.SubscriptionIdIn))
                return null;

            return Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(model.SubscriptionIdIn, "subscriptionIdIn", true);
        }

        public static List<string> getExternalIdIn(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.ExternalIdIn))
                return null;

            return model.ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public static HashSet<SubscriptionType> GetSubscriptionTypeIn(this KalturaSubscriptionFilter model)
        {
            if (string.IsNullOrEmpty(model.DependencyTypeIn))
                return null;
            var types = Utils.Utils.ParseCommaSeparatedValues<HashSet<KalturaSubscriptionDependencyType>, KalturaSubscriptionDependencyType>(model.DependencyTypeIn, "assetTypeIn", true, true);
            var mapped = AutoMapper.Mapper.Map<HashSet<SubscriptionType>>(types);
            return mapped;
        }
    }
}