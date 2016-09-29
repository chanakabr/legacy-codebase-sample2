using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    [OldStandard("paymentGatewayConfiguration", "payment_gatewaye_configuration")]
    public class KalturaPaymentGatewayConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Payment gateway configuration
        /// </summary>
        [DataMember(Name = "paymentGatewayConfiguration")]
        [JsonProperty("paymentGatewayConfiguration")]
        [XmlArray(ElementName = "paymentGatewayConfiguration", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaKeyValue> Configuration { get; set; }
    }
}