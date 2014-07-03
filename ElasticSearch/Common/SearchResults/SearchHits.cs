using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common.SearchResults
{
    public class SearchHits
    {
        [JsonProperty("hits")]
        public HitStatus Hits;
        [JsonIgnore]
        [JsonProperty("_shards")]
        public ShardStatus ShardStatus;
    }
}
