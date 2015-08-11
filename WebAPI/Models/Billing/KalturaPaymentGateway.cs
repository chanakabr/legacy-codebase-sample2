using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    public class KalturaPaymentGateway : KalturaOTTObject
    {
        /// <summary>
        /// payment gateway 
        /// </summary>
        [DataMember(Name = "payment_gateway")]
        [JsonProperty("payment_gateway")]
        [XmlElement(ElementName = "payment_gateway")]
        public KalturaPaymentGatewayBaseProfile paymentGateway { get; set; }

        /// <summary>
        /// distinction payment gateway selected by account or household
        /// </summary>
        [DataMember(Name = "selected_by")]
        [JsonProperty("selected_by")]
        [XmlElement(ElementName = "selected_by")]
        public KalturaHouseholdPaymentGatewaySelectedBy selectedBy { get; set; }
    }
}