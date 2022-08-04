using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Coupon promotion
    /// </summary>
    [SchemeClass(Required = new string[] { "couponGroupId" })]
    public partial class KalturaCouponPromotion : KalturaBasePromotion
    {
        /// <summary>
        /// CouponGroup identifier
        /// </summary>
        [DataMember(Name = "couponGroupId")]
        [JsonProperty("couponGroupId")]
        [XmlElement(ElementName = "couponGroupId")]
        [SchemeProperty(MinLong = 1)]
        public long CouponGroupId { get; set; }
    }
}