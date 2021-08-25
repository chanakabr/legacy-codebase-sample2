using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Searcher.Settings
{
    [JsonObject()]
    public class Tokenizer
    {
        [JsonProperty()]
        public string type;
    }

    [JsonObject()]
    public class KuromojiTokenizer : Tokenizer
    {
        [JsonProperty()]
        public string mode;
        [JsonProperty()]
        public bool? discard_punctuation;
        [JsonProperty()]
        public List<string> user_dictionary;

        public KuromojiTokenizer()
        {
        }
    }
}
