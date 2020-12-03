using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Entitlement discount details
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaCouponEntitlementDiscountDetails))]
    public partial class KalturaEntitlementDiscountDetails : KalturaOTTObject
    {
        /// <summary>
        /// Amount
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount")]
        [SchemeProperty(ReadOnly = true)]
        public double Amount { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? EndDate { get; set; }
    }

    /// <summary>
    /// Coupon discount details
    /// </summary>
    [Serializable]
    public partial class KalturaCouponEntitlementDiscountDetails : KalturaEntitlementDiscountDetails
    {
        /// <summary>
        /// Coupon Code
        /// </summary>
        [DataMember(Name = "couponCode")]
        [JsonProperty("couponCode")]
        [XmlElement(ElementName = "couponCode")]
        [SchemeProperty(ReadOnly = true)]
        public string CouponCode { get; set; }

        /// <summary>
        /// Endless coupon
        /// </summary>
        [DataMember(Name = "endlessCoupon")]
        [JsonProperty("endlessCoupon")]
        [XmlElement(ElementName = "endlessCoupon")]
        [SchemeProperty(ReadOnly = true)]
        public bool EndlessCoupon { get; set; }
    }

    /// <summary>
    ///  
    /// </summary>
    [Serializable]
    public partial class KalturaEntitlementDiscountDetailsIdentifier : KalturaEntitlementDiscountDetails
    {
        /// <summary>
        ///Identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }
    }

    /// <summary>
    ///  Compensation entitlement discount details
    /// </summary>
    [Serializable]
    public partial class KalturaCompensationEntitlementDiscountDetails : KalturaEntitlementDiscountDetailsIdentifier { }

    /// <summary>
    ///  Campaign entitlement discount details
    /// </summary>
    [Serializable]
    public partial class KalturaCampaignEntitlementDiscountDetails : KalturaEntitlementDiscountDetailsIdentifier { }

    /// <summary>
    ///  Discount entitlement discount details
    /// </summary>
    [Serializable]
    public partial class KalturaDiscountEntitlementDiscountDetails : KalturaEntitlementDiscountDetailsIdentifier { }


    /// <summary>
    ///  Trail entitlement discount details
    /// </summary>
    [Serializable]
    public partial class KalturaTrailEntitlementDiscountDetails : KalturaEntitlementDiscountDetailsIdentifier { }
}