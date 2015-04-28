using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    public class SearchAssets
    {
        /// <summary>
        /// Filter types
        /// </summary>
        [DataMember(Name = "filter_types")]
        [JsonProperty(PropertyName = "filter_types")]
        public List<int> filter_types { get; set; }

        /// <summary>
        /// Filter
        /// </summary>
        [DataMember(Name = "filter")]
        [JsonProperty(PropertyName = "filter")]
        public string filter { get; set; }

        /// <summary>
        /// Order by
        /// </summary>
        [DataMember(Name = "order_by")]
        [JsonProperty(PropertyName = "order_by")]
        public Order? order_by { get; set; }

        /// <summary>
        /// With
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        public List<string> with { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        [DataMember(Name = "page_size")]
        [JsonProperty(PropertyName = "page_size")]
        public int page_size { get; set; }

        /// <summary>
        /// Page index
        /// </summary>
        [DataMember(Name = "page_index")]
        [JsonProperty(PropertyName = "page_index")]
        public int page_index { get; set; }


    }
}