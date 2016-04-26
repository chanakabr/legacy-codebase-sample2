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
    public class KalturaRecordingFilter : KalturaOTTObject
    {

        /// <summary>
        /// Recording Statuses
        /// </summary>
        [DataMember(Name = "recording_statuses")]
        [JsonProperty(PropertyName = "recording_statuses")]
        [XmlArray(ElementName = "recording_statuses", IsNullable = true)]
        [XmlArrayItem(ElementName = "recording_statuses")]
        public List<KalturaRecordingStatusHolder> RecordingStatuses { get; set; }

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "filter_expression")]
        [JsonProperty("filter_expression")]
        [XmlElement(ElementName = "filter_expression")]
        public string filter_expression { get; set; }
    }
}