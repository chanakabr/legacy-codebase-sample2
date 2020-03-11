using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Premium services list
    /// </summary>
    [DataContract(Name = "PremiumServices", Namespace = "")]
    [XmlRoot("PremiumServices")]
    public partial class KalturaHouseholdPremiumServiceListResponse : KalturaListResponse
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