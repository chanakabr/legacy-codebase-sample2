using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering epg channels
    /// </summary>
    [Serializable]
    [OldStandard("startTime", "start_time")]
    [OldStandard("endTime", "end_time")]
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
        public long? StartTime { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        [DataMember(Name = "endTime")]
        [JsonProperty(PropertyName = "endTime")]
        [XmlElement("endTime")]
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