using Newtonsoft.Json;
using System.Collections.Generic;
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
        /// Indicates whether the payment method allow multiple instances 
        /// </summary>
        [DataMember(Name = "allow_multi_instance")]
        [JsonProperty("allow_multi_instance")]
        [XmlElement(ElementName = "allow_multi_instance")]
        public bool AllowMultiInstance { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        [DataMember(Name = "household_payment_methods")]
        [JsonProperty("household_payment_methods")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaHouseholdPaymentMethod> HouseholdPaymentMethods { get; set; }
    }
}