using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects;

namespace Core.Pricing
{
    public class PpvModuleInternal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string alias { get; set; }
        public string AdsParam { get; set; }
        public string ProductCode { get; set; }
        public int? PriceId { get; set; }
        public long? DiscountId { get; set; }
        public long? UsageModuleId { get; set; }
        public long? CouponsGroupId { get; set; }
        public LanguageContainer[] Description { get; set; }
        public bool? SubscriptionOnly { get; set; }
        public bool? IsActive { get; set; }
        public bool? FirstDeviceLimitation { get; set; }
        public List<int> RelatedFileTypes{ get; set; }
        public AdsPolicy? AdsPolicy { get; set; }
        public long? VirtualAssetId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public long? AssetUserRuleId { get; set; }


        public bool ShouldUpdate(PPVModule oldPPVModule)
        {
            bool shouldUpdate = false;

            if (!string.IsNullOrEmpty(Name) && !string.Equals(Name, oldPPVModule.m_sObjectVirtualName))
            {
                shouldUpdate = true;
            }
            else
            {
                Name = oldPPVModule.m_sObjectVirtualName;
            }
            
            if (!string.IsNullOrEmpty(alias) && !string.Equals(alias, oldPPVModule.alias))
            {
                shouldUpdate = true;
            }
            else
            {
                alias = oldPPVModule.alias;
            }
            
            if (!string.IsNullOrEmpty(AdsParam) && !string.Equals(AdsParam, oldPPVModule.AdsParam))
            {
                shouldUpdate = true;
            }
            else
            {
                AdsParam = oldPPVModule.AdsParam;
            }
            
            if (ProductCode != null && !string.Equals(ProductCode, oldPPVModule.m_Product_Code))
            {
                shouldUpdate = true;
            }
            else
            {
                ProductCode = oldPPVModule.m_Product_Code;
            }
            
            if (PriceId.HasValue && (oldPPVModule.m_oPriceCode != null &&PriceId != oldPPVModule.m_oPriceCode.m_nObjectID || oldPPVModule.m_oPriceCode == null))
            {
                shouldUpdate = true;
            }
            else if (oldPPVModule.m_oPriceCode != null)
            {
                PriceId = oldPPVModule.m_oPriceCode.m_nObjectID;
            }
            if (UsageModuleId.HasValue && (oldPPVModule.m_oUsageModule != null && UsageModuleId != oldPPVModule.m_oUsageModule.m_nObjectID || oldPPVModule.m_oUsageModule == null))
            {
                shouldUpdate = true;
            }
            else if (oldPPVModule.m_oUsageModule != null)
            {
                UsageModuleId = oldPPVModule.m_oUsageModule.m_nObjectID;
            }
            
            if (DiscountId.HasValue && ((oldPPVModule.m_oDiscountModule != null && DiscountId != oldPPVModule.m_oDiscountModule.m_nObjectID) 
                                        || oldPPVModule.m_oDiscountModule == null))
            {
                shouldUpdate = true;
            }
            else if(oldPPVModule.m_oDiscountModule != null)
            {
                DiscountId = oldPPVModule.m_oDiscountModule.m_nObjectID;
            }

            long couponGroupIdLong = 0;
           
            if (CouponsGroupId.HasValue  && ((oldPPVModule.m_oCouponsGroup == null || 
                                              oldPPVModule.m_oCouponsGroup != null && long.TryParse(oldPPVModule.m_oCouponsGroup.m_sGroupCode, out couponGroupIdLong) &&
                                              CouponsGroupId.Value != couponGroupIdLong)))
            {
                shouldUpdate = true;
            }
            else if (oldPPVModule.m_oCouponsGroup != null)
            {
                CouponsGroupId = couponGroupIdLong;
            }

            if (SubscriptionOnly != null && SubscriptionOnly !=oldPPVModule.m_bSubscriptionOnly)
            {
                shouldUpdate = true;
            }
            else
            {
                SubscriptionOnly = oldPPVModule.m_bSubscriptionOnly;
            }
            
            if (IsActive != null && IsActive != oldPPVModule.IsActive)
            {
                shouldUpdate = true;
            }
            else
            {
                IsActive = oldPPVModule.IsActive;
            }
            
            if (FirstDeviceLimitation != null && FirstDeviceLimitation != oldPPVModule.m_bFirstDeviceLimitation)
            {
                shouldUpdate = true;
            }
            else
            {
                FirstDeviceLimitation = oldPPVModule.m_bFirstDeviceLimitation;
            }          
            if (AdsPolicy != null && AdsPolicy != oldPPVModule.AdsPolicy)
            {
                shouldUpdate = true;
            }
            else
            {
                AdsPolicy = oldPPVModule.AdsPolicy;
            }
            
            return shouldUpdate;
        }

        public bool ShouldUpdateFileTypes(PPVModule oldPPVModule)
        {
            if (RelatedFileTypes != null && (
                (RelatedFileTypes?.Count != 0 && oldPPVModule.m_relatedFileTypes == null) ||
                (oldPPVModule.m_relatedFileTypes != null && !Enumerable.SequenceEqual(oldPPVModule.m_relatedFileTypes,
                    RelatedFileTypes))))
            {
                return true;
            }

            return false;
        }
        
        public bool ShouldUpdateDescription(PPVModule oldPPVModule)
        {
            if (Description != null && (
                (Description.Length != 0 && oldPPVModule.m_sDescription == null) ||
                (oldPPVModule.m_sDescription != null && !Enumerable.SequenceEqual(oldPPVModule.m_sDescription, Description))))
            {
                return true;
            }

            return false;
        }
    }
    
}