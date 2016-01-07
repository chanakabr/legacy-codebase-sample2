using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    public class KalturaPaymentGatewayConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Payment gateway configuration
        /// </summary>
        [DataMember(Name = "payment_gatewaye_configuration")]
        [JsonProperty("payment_gatewaye_configuration")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaKeyValue> Configuration { get; set; }
    }
}