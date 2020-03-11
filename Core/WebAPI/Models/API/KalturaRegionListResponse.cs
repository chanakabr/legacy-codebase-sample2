using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Regions list
    /// </summary>
    [DataContract(Name = "Regions", Namespace = "")]
    [XmlRoot("Regions")]
    public partial class KalturaRegionListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of regions
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaRegion> Regions { get; set; }
    }
}