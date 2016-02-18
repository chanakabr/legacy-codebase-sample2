using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// payment method
    /// </summary>
    public class KalturaPaymentMethod : KalturaOTTObject
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
        /// Selected payment method 
        /// </summary>
        [DataMember(Name = "selected")]
        [JsonProperty("selected")]
        [XmlElement(ElementName = "selected")]
        public bool Selected { get; set; }
    }
}