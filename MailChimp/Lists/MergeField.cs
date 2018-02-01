using Newtonsoft.Json;
using System.Collections.Generic;

namespace MailChimp.Lists
{
    public class MergeField
    {
        [JsonProperty("merge_id")]
        public int Id { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("default_value")]
        public string DefaultValue { get; set; }
    }

    public class CollectionMergeField
    {
        [JsonProperty("merge_fields")]
        public List<MergeField> Lists { get; set; }
        [JsonProperty("total_items")]
        public int TotalItems { get; set; }
    }
}
