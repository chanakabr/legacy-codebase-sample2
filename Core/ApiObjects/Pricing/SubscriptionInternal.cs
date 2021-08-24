using ApiObjects.Base;
using ApiObjects.Pricing.Dto;
using System;
using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class SubscriptionInternal : BaseSupportsNullable
    {
        public long Id { get; set; }

        public List<long> ChannelsIds { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<long> FileTypesIds { get; set; }

        public long? InternalDiscountModuleId { get; set; }

        public LanguageContainer[] Names { get; set; }

        public LanguageContainer[] Descriptions { get; set; }

        public long? ProrityInOrder { get; set; }

        public List<long> PricePlanIds { get; set; }

        public long PreviewModuleId { get; set; }

        public int? HouseholdLimitationsId { get; set; }

        public int? GracePeriodMinutes { get; set; }

        public ServiceObject[] PremiumServices { get; set; }       

        public List<SubscriptionCouponGroupDTO> CouponGroups { get; set; }

        public List<KeyValuePair<VerificationPaymentGateway, string>> ExternalProductCodes;

        public SubscriptionType DependencyType { get; set; }

        public string ExternalId { get; set; }

        public bool IsCancellationBlocked { get; set; }

        public long? PreSaleDate { get; set; }

        public AdsPolicy? AdsPolicy { get; set; }

        public string AdsParams { get; set; }

        public bool? IsActive { get; set; }
    }
}