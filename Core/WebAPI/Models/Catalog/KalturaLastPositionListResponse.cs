using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// List of last positions
    /// </summary>
    [Obsolete]
    [DataContract(Name = "LastPosition", Namespace = "")]
    [XmlRoot("LastPosition")]
    public partial class KalturaLastPositionListResponse : KalturaListResponse
    {
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaLastPosition> LastPositions { get; set; }
    }
}