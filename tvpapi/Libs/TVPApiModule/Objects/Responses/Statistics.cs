using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Statistics
    {
        [JsonProperty(PropertyName = "asset_id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "likes")]
        public int Likes { get; set; }

        [JsonProperty(PropertyName = "views")]
        public int Views { get; set; }

        [JsonProperty(PropertyName = "rating_count")]
        public int RatingCount { get; set; }

        [JsonProperty(PropertyName = "rating")]
        public double Rating { get; set; }
    }
}
