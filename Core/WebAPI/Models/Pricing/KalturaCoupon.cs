using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupon details container
    /// </summary>
    public partial class KalturaCoupon : KalturaOTTObject
    {
        /// <summary>
        /// Coupons group details 
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("coupons_group")]
        public KalturaCouponsGroup CouponsGroup { get; set; }

        /// <summary>
        /// Coupon status 
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCouponStatus Status { get; set; }

        /// <summary>
        /// Total available coupon uses
        /// </summary>
        [DataMember(Name = "totalUses")]
        [JsonProperty("totalUses")]
        [XmlElement(ElementName = "totalUses")]
        [SchemeProperty(ReadOnly = true)]
        public int? TotalUses { get; set; }

        /// <summary>
        /// Left coupon uses
        /// </summary>
        [DataMember(Name = "leftUses")]
        [JsonProperty("leftUses")]
        [XmlElement(ElementName = "leftUses")]
        [SchemeProperty(ReadOnly = true)]
        public int? LeftUses { get; set; }

        /// <summary>
        /// Coupon code
        /// </summary>
        [DataMember(Name = "couponCode")]
        [JsonProperty("couponCode")]
        [XmlElement(ElementName = "couponCode")]
        [SchemeProperty(ReadOnly = true)]
        public string CouponCode { get; set; }
    }
}