using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
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
}