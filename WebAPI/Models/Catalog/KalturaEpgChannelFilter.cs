using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering epg channels
    /// </summary>
    [Serializable]
    [Obsolete]
    public class KalturaEpgChannelFilter : KalturaOTTObject
    {
        /// <summary>
        /// Entities IDs
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty(PropertyName = "ids")]
        [XmlArray(ElementName = "ids", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> IDs { get; set; }

        /// <summary>
        /// Start Time
        /// </summary>
        [DataMember(Name = "startTime")]
        [JsonProperty(PropertyName = "startTime")]
        [XmlElement("startTime")]
        [OldStandardProperty("start_time")]
        public long? StartTime { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        [DataMember(Name = "endTime")]
        [JsonProperty(PropertyName = "endTime")]
        [XmlElement("endTime")]
        [OldStandardProperty("end_time")]
        public long? EndTime { get; set; }

        internal long getStartTime()
        {
            return StartTime.HasValue ? (long)StartTime : 0;
        }

        internal long getEndTime()
        {
            return EndTime.HasValue ? (long)EndTime : 0;
        }
    }
}