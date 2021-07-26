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

    [JsonObject()]
    public class AnalyzerDefinitions
    {
        [JsonProperty()]
        public Dictionary<string, Analyzer> Analyzers;
    }
}

//"filter" : [ "lowercase", "icu_folding", "icu_normalizer", "asciifolding" ],
//              "char_filter" : [ "html_strip" ],
//              "type" : "custom",
//              "tokenizer" : "keyword"