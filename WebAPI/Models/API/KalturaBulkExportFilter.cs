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
    /// Bulk export tasks filter
    /// </summary>
    public class KalturaBulkExportFilter : KalturaOTTObject
    {
        /// <summary>
        /// Defines whether to filter the tasks by identifier or external key
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaBulkExportReferenceBy By { get; set; }

        /// <summary>
        /// The tasks identifiers or external keys - depends on "by" parameter
        /// </summary>
        [DataMember(Name = "tasks")]
        [JsonProperty("tasks")]
        [XmlArray(ElementName = "objects")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> Tasks{ get; set; }
    }
}