using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    public class KalturaPaymentMethodProfile : KalturaOTTObject
    {
        /// <summary>
        /// Payment method identifier (internal)
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether the payment method allow multiple instances 
        /// </summary>
        [DataMember(Name = "allow_multi_instance")]
        [JsonProperty("allow_multi_instance")]
        [XmlElement(ElementName = "allow_multi_instance")]
        public bool AllowMultiInstance { get; set; }
    }
}