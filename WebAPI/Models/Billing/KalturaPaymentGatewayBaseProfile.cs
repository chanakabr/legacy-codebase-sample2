using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Billing
{
    /// <summary>
    /// Payment gateway base profile
    /// </summary>
    public class KalturaPaymentGatewayBaseProfile : KalturaOTTObject
    {
        /// <summary>
        /// payment gateway id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// payment gateway name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Payment gateway default (true/false)
        /// </summary>
        [DataMember(Name = "is_default")]
        [JsonProperty("is_default")]
        [XmlElement(ElementName = "is_default")]
        public bool IsDefault { get; set; }

        /// <summary>
        /// distinction payment gateway selected by account or household
        /// </summary>
        [DataMember(Name = "selected_by")]
        [JsonProperty("selected_by")]
        [XmlElement(ElementName = "selected_by", IsNullable = true)]
        public KalturaHouseholdPaymentGatewaySelectedBy selectedBy { get; set; }
    }
}
