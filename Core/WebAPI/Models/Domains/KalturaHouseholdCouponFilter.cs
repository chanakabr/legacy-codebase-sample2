using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Models.Domains
{
    [Serializable]
    public partial class KalturaHouseholdCouponFilter : KalturaFilter<KalturaHouseholdCouponOrderBy>
    {
        /// <summary>
        /// Indicates which household coupons list to return by their business module type.
        /// </summary>
        [DataMember(Name = "businessModuleTypeEqual")]
        [JsonProperty("businessModuleTypeEqual")]
        [XmlElement(ElementName = "businessModuleTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaTransactionType BusinessModuleTypeEqual { get; set; }

        /// <summary>
        /// Indicates which household coupons list to return by their business module ID.
        /// </summary>
        [DataMember(Name = "businessModuleIdEqual")]
        [JsonProperty("businessModuleIdEqual")]
        [XmlElement(ElementName = "businessModuleIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinInteger = 1)]
        public long BusinessModuleIdEqual { get; set; }

        /// <summary>
        /// Allow clients to inquiry if a specific coupon is part of an HH’s wallet or not
        /// </summary>
        [DataMember(Name = "couponCode")]
        [JsonProperty("couponCode")]
        [XmlElement(ElementName = "couponCode", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinLength = 1)]
        public string CouponCode { get; set; }
        /// <summary>
        /// Allow clients to filter out coupons which are valid/invalid
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaCouponStatus? Status { get; set; }

        public override KalturaHouseholdCouponOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdCouponOrderBy.NONE;
        }
    }
}