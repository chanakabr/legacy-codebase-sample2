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
    /// Filtering recordings
    /// </summary>
    [Serializable]
    public class KalturaRecordingFilter : KalturaFilter
    {

        /// <summary>
        /// Recording Statuses
        /// </summary>
        [DataMember(Name = "statusIn")]
        [JsonProperty(PropertyName = "statusIn")]
        [XmlArray(ElementName = "statusIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "statusIn")]
        public List<KalturaRecordingStatusHolder> StatusIn { get; set; }

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