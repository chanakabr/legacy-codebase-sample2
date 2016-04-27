using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    public class KalturaRecordingFilter : KalturaRequestFilter
    {

        /// <summary>
        /// Recording Statuses
        /// </summary>
        [DataMember(Name = "recordingStatuses")]
        [JsonProperty(PropertyName = "recordingStatuses")]
        [XmlArray(ElementName = "recordingStatuses", IsNullable = true)]
        [XmlArrayItem(ElementName = "recordingStatuses")]
        public List<KalturaRecordingStatusHolder> RecordingStatuses { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression", IsNullable = true)]
        public string FilterExpression { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        public KalturaRecordingOrder? OrderBy { get; set; }
        
    }
}