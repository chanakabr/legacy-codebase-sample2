using System.Collections.Generic;
using System.Runtime.Serialization;
using Nest;
using Newtonsoft.Json;

namespace ElasticSearch.Searcher.Settings
{
    public class CustomProperty
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class SortProperty : CustomProperty
    {
        [JsonProperty("index")]
        public bool? Index { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("variant")]
        public string Variant { get; set; }
    }

    public class IcuCollationKeywordProperty : IProperty
    {
        public string Type { get; set; } = "icu_collation_keyword";

        public PropertyName Name { get; set; }

        public IDictionary<string, string> Meta { get; set; }

        public IDictionary<string, object> LocalMetadata { get; set; }

        [DataMember(Name = "index")]
        public bool? Index { get; set; }

        [DataMember(Name = "language")]
        public string Language { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "variant")]
        public string Variant { get; set; }

        public IcuCollationKeywordProperty(string name)
        {
            Name = new PropertyName(name);
        }
    }
}