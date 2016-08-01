using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    [OldStandard("paymentGatewayeConfiguration", "payment_gatewaye_configuration")]
    public class KalturaPaymentGatewayConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// Payment gateway configuration
        /// </summary>
        [DataMember(Name = "paymentGatewayeConfiguration")]
        [JsonProperty("paymentGatewayeConfiguration")]
        [XmlArray(ElementName = "paymentGatewayeConfiguration", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaKeyValue> Configuration { get; set; }
    }
}