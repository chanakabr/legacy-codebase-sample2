using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Premium services list
    /// </summary>
    [DataContract(Name = "PremiumServices", Namespace = "")]
    [XmlRoot("PremiumServices")]
    public class KalturaHouseholdPremiumServiceListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of premium services
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdPremiumService> PremiumServices { get; set; }
    }
}