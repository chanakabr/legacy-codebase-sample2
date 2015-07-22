using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Buzz score
    /// </summary>
    public class KalturaBuzzScore
    {
        /// <summary>
        /// Normalized average score 
        /// </summary>
        [DataMember(Name = "normalized_avg_score")]
        [JsonProperty(PropertyName = "normalized_avg_score")]
        public double NormalizedAvgScore { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "update_date")]
        [JsonProperty(PropertyName = "update_date")]
        public DateTime UpdateDate { get; set; }

        /// <summary>
        /// Average score
        /// </summary>
        [DataMember(Name = "avg_score")]
        [JsonProperty(PropertyName = "avg_score")]
        public double AvgScore { get; set; }
    }
}