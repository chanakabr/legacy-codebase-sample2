using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public partial class KalturaPublicCouponGenerationOptions : KalturaCouponGenerationOptions
    {
        /// <summary>
        /// Coupon code (name)
        /// </summary>
        [DataMember(Name = "code")]
        [JsonProperty("code")]
        [XmlElement(ElementName = "code", IsNullable = false)]
        public string Code { get; set; }
    }
}
