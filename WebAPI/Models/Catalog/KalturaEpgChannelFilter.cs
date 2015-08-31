using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering epg channels
    /// </summary>
    [Serializable]
    public class KalturaEpgChannelFilter : KalturaOTTObject
    {
        /// <summary>
        /// Entities IDs
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "ids")]
        [XmlArrayItem(ElementName = "item")]        
        public List<KalturaIntegerValue> IDs { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        [DataMember(Name = "start_time")]
        [JsonProperty(PropertyName = "start_time")]
        [XmlElement("start_time")]
        public long StartTime { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        [DataMember(Name = "end_time")]
        [JsonProperty(PropertyName = "end_time")]
        [XmlElement("end_time")]
        public long EndTime { get; set; }
    }
}