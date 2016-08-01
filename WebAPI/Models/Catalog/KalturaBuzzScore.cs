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
    [OldStandard("normalizedAvgScore", "normalized_avg_score")]
    [OldStandard("updateDate", "update_date")]
    [OldStandard("avgScore", "avg_score")]
    public class KalturaBuzzScore : KalturaOTTObject
    {
        /// <summary>
        /// Normalized average score 
        /// </summary>
        [DataMember(Name = "normalizedAvgScore")]
        [JsonProperty(PropertyName = "normalizedAvgScore")]
        [XmlElement(ElementName = "normalizedAvgScore")]
        public double? NormalizedAvgScore { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        public long? UpdateDate { get; set; }

        /// <summary>
        /// Average score
        /// </summary>
        [DataMember(Name = "avgScore")]
        [JsonProperty(PropertyName = "avgScore")]
        [XmlElement(ElementName = "avgScore")]
        public double? AvgScore { get; set; }
    }
}