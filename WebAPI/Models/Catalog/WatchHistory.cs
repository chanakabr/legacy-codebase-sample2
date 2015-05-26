using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    public class WatchHistory
    {
        /// <summary>
        /// List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "filter_types")]
        [JsonProperty(PropertyName = "filter_types")]
        public List<int> filter_types { get; set; }

        /// <summary>
        /// Which type of recently watched media to include in the result – those that finished watching, those that are in progress or both.
        /// If omitted or specified filter = all – return all types.
        /// Allowed values: progress – return medias that are in-progress, done – return medias that finished watching.
        /// </summary>
        [DataMember(Name = "filter_status")]
        [JsonProperty(PropertyName = "filter_status")]
        public WatchStatus? filter_status { get; set; }

        /// <summary>
        /// How many days back to return the watched media. If omitted, default to 7 days
        /// </summary>
        [DataMember(Name = "days")]
        [JsonProperty(PropertyName = "days")]
        public int days { get; set; }

        /// <summary>
        /// Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset.
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        public List<With> with { get; set; }

        /// <summary>
        /// Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50
        /// </summary>
        [DataMember(Name = "page_size")]
        [JsonProperty(PropertyName = "page_size")]
        public int? page_size { get; set; }
        
        /// <summary>
        /// Page number to return. If omitted will return first page
        /// </summary>
        [DataMember(Name = "page_index")]
        [JsonProperty(PropertyName = "page_index")]
        public int page_index { get; set; }
    }
}