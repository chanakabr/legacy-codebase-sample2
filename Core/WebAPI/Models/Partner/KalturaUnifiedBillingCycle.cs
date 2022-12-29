using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaUnifiedBillingCycle : KalturaOTTObject
    {
        /// <summary>
        /// UnifiedBillingCycle name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// cycle duration
        /// </summary>
        [DataMember(Name = "duration")]
        [JsonProperty("duration")]
        [XmlElement(ElementName = "duration")]
        public KalturaDuration Duration { get; set; }

        /// <summary>
        /// Payment Gateway Id
        /// </summary>
        [DataMember(Name = "paymentGatewayId")]
        [JsonProperty("paymentGatewayId")]
        [XmlElement(ElementName = "paymentGatewayId", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Define if partial billing shall be calculated or not
        /// </summary>
        [DataMember(Name = "ignorePartialBilling")]
        [JsonProperty("ignorePartialBilling")]
        [XmlElement(ElementName = "ignorePartialBilling", IsNullable = true)]
        public bool? IgnorePartialBilling { get; set; }
    }
}