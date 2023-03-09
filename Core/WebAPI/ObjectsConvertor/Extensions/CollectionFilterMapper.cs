using System;
using System.Collections.Generic;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class CollectionFilterMapper
    {
        public static string[] getCollectionIdIn(this KalturaCollectionFilter model)
        {
            if (string.IsNullOrEmpty(model.CollectionIdIn))
                return null;

            return model.CollectionIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static HashSet<long> GetAssetUserRuleIdIn(this KalturaCollectionFilter model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<HashSet<long>, long>(model.AssetUserRuleIdIn, "KalturaCollectionFilter.assetUserRuleIdIn", true);
        }
    }
}