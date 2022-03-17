using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupons group details
    /// </summary>
    public partial class KalturaSubscriptionCouponGroup : KalturaOTTObject
    {
        /// <summary>
        /// Coupon group identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinLong = 1)]
        public long CouponGroupId { get; set; }

        /// <summary>
        /// The first date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? EndDate { get; set; }
    }
}