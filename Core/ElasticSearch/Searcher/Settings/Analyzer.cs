using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Searcher.Settings
{
    [JsonObject()]
    public class Analyzer
    {
        [JsonProperty()]
        public List<string> filter;
        [JsonProperty()]
        public List<string> char_filter;
        [JsonProperty()]
        public string tokenizer;
    }
}

//"filter" : [ "lowercase", "icu_folding", "icu_normalizer", "asciifolding" ],
//              "char_filter" : [ "html_strip" ],
//              "type" : "custom",
//              "tokenizer" : "keyword"