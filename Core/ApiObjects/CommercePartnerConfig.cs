using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class CommercePartnerConfig
    {
        public Dictionary<eTransactionType, int> BookmarkEventThresholds { get; set; }
        public bool? KeepSubscriptionAddOns { get; set; }
        public int? ProgramAssetEntitlementPaddingStart { get; set; }
        public int? ProgramAssetEntitlementPaddingEnd { get; set; }

        public bool SetUnchangedProperties(CommercePartnerConfig oldConfig)
        {
            var needToUpdate = false;
            if (BookmarkEventThresholds != null)
            {
                needToUpdate = true;
            }
            else
            {
                BookmarkEventThresholds = oldConfig.BookmarkEventThresholds;
            }

            if (KeepSubscriptionAddOns.HasValue)
            {
                needToUpdate = true;
            }
            else
            {
                KeepSubscriptionAddOns = oldConfig.KeepSubscriptionAddOns;
            }

            if (ProgramAssetEntitlementPaddingStart.HasValue && ProgramAssetEntitlementPaddingStart != oldConfig.ProgramAssetEntitlementPaddingStart)
            {
                needToUpdate = true;
            }
            else
            {
                ProgramAssetEntitlementPaddingStart = oldConfig.ProgramAssetEntitlementPaddingStart;
            }
            
            if (ProgramAssetEntitlementPaddingEnd.HasValue && ProgramAssetEntitlementPaddingEnd != oldConfig.ProgramAssetEntitlementPaddingEnd)
            {
                needToUpdate = true;
            }
            else
            {
                ProgramAssetEntitlementPaddingEnd = oldConfig.ProgramAssetEntitlementPaddingEnd;
            }
            
            return needToUpdate;
        }
    }
}