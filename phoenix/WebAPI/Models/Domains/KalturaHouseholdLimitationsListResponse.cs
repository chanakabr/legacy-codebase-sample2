using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    /// <summary>
    /// Household limitations details 
    /// </summary>
    public partial class KalturaHouseholdLimitationsListResponse : KalturaListResponse
    {

        /// <summary>
        /// Household limitations
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaHouseholdLimitations> Objects { get; set; }

    }
}