using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Buzz score
    /// </summary>
    public class KalturaBuzzScore : KalturaOTTObject
    {
        /// <summary>
        /// Normalized average score 
        /// </summary>
        [DataMember(Name = "normalizedAvgScore")]
        [JsonProperty(PropertyName = "normalizedAvgScore")]
        [XmlElement(ElementName = "normalizedAvgScore")]
        [OldStandardProperty("normalized_avg_score")]
        public double? NormalizedAvgScore { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [OldStandardProperty("update_date")]
        public long? UpdateDate { get; set; }

        /// <summary>
        /// Average score
        /// </summary>
        [DataMember(Name = "avgScore")]
        [JsonProperty(PropertyName = "avgScore")]
        [XmlElement(ElementName = "avgScore")]
        [OldStandardProperty("avg_score")]
        public double? AvgScore { get; set; }
    }
}