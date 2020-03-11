using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common.SearchResults
{
    public class ShardStatus
    {
        [JsonProperty("failed")]
        public int Failed;
        [JsonProperty("successful")]
        public int Successful;
        [JsonProperty("total")]
        public int Total;
    }
}
