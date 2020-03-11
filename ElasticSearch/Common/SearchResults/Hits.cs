using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common.SearchResults
{
    public class Hits
    {
        [JsonProperty("_source")]
        public Dictionary<string, string> Source = new Dictionary<string, string>();

        [JsonProperty("fields")]
        public Dictionary<string, string> Fields;

        [JsonProperty("_id")]
        public string Id;
        [JsonProperty("_index")]
        public string Index;
        [JsonProperty("_score")]
        public string Score;
        [JsonProperty("_type")]
        public string Type;
        [JsonProperty("_version")]
        public int Version;
    }
}
