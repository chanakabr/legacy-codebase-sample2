using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Reports info wrapper
    /// </summary>
    public partial class KalturaReportListResponse : KalturaListResponse
    {
        /// <summary>
        /// Reports
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaReport> Objects { get; set; }

    }

}