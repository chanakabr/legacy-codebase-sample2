using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Base assets request parameters
    /// </summary>
    [Serializable]
    public class KalturaBaseAssetsRequest
    {
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

        /// <summary>
        /// Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        public List<KalturaWith> with { get; set; }
    }
}