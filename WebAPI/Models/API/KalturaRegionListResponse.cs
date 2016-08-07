using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Regions list
    /// </summary>
    [DataContract(Name = "Regions", Namespace = "")]
    [XmlRoot("Regions")]
    public class KalturaRegionListResponse : KalturaListResponse
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