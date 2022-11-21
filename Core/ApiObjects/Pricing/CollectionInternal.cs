using ApiObjects.Base;
using ApiObjects.Pricing.Dto;
using System;
using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class CollectionInternal : BaseSupportsNullable
    {
        public long Id { get; set; }

        public long? PriceDetailsId { get; set; } //  collectionToInsert.m_oCollectionPriceCode.m_nObjectID,
        
        public int? DiscountModuleId { get; set; }//  discountId, 

        public int? UsageModuleId { get; set; } //  collectionToInsert.m_oCollectionUsageModule.m_nObjectID, 

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public LanguageContainer[] Names { get; set; }
        
        public LanguageContainer[] Descriptions { get; set; }

        public List<long> ChannelsIds { get; set; }

        public List<KeyValuePair<VerificationPaymentGateway, string>> ExternalProductCodes;
        
        public bool? IsActive { get; set; }
        
        public string ExternalId { get; set; }

        public List<SubscriptionCouponGroupDTO> CouponGroups { get; set; }

        public List<long> FileTypesIds { get; set; }

        public long? AssetUserRuleId { get; set; }
    }
}