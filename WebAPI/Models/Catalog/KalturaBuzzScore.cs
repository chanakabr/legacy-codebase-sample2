using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
        [DataMember(Name = "normalized_avg_score")]
        [JsonProperty(PropertyName = "normalized_avg_score")]
        [XmlElement(ElementName = "normalized_avg_score")]
        public double NormalizedAvgScore { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "update_date")]
        [JsonProperty(PropertyName = "update_date")]
        [XmlElement(ElementName = "update_date")]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Average score
        /// </summary>
        [DataMember(Name = "avg_score")]
        [JsonProperty(PropertyName = "avg_score")]
        [XmlElement(ElementName = "avg_score")]
        public double AvgScore { get; set; }
    }
}