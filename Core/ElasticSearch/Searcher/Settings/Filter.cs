using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Searcher.Settings
{
    [JsonObject()]
    public class Filter
    {
        [JsonProperty()]
        public List<string> token_chars;
        [JsonProperty()]
        public string type;
    }

    [JsonObject()]
    public class NgramFilter : Filter
    {
        [JsonProperty()]
        public int min_gram;
        [JsonProperty()]
        public int max_gram;
    }

    [JsonObject()]
    public class FilterDefinitions
    {
        [JsonProperty()]
        public Dictionary<string, Filter> filters;
    }
}

//    "edgengram_filter" : {
//        "token_chars" : [ "letter", "digit", "punctuation", "symbol" ],
//              "min_gram" : "1",
//              "type" : "edgeNGram",
//              "max_gram" : "20"
//            }