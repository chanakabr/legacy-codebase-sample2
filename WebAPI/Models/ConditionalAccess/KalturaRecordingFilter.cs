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
    public class KalturaRecordingFilter : KalturaBaseFilter
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
        [XmlElement(ElementName = "filter_expression", IsNullable = true)]
        public string filter_expression { get; set; }

        /// <summary>
        /// with
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        [XmlArray(ElementName = "with", IsNullable = true)]
        [XmlArrayItem(ElementName = "with")]
        public List<KalturaCatalogWithHolder> with { get; set; }

        /// <summary>
        /// request ID
        /// </summary>
        [DataMember(Name = "request_id")]
        [JsonProperty("request_id")]
        [XmlElement(ElementName = "request_id", IsNullable = true)]
        public string request_id { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "order_by")]
        [JsonProperty("order_by")]
        [XmlElement(ElementName = "order_by", IsNullable = true)]
        public KalturaRecordingOrder? order_by { get; set; }
        
    }
}