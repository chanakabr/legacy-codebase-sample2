using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class SegmentValue
    {
        [JsonProperty()]
        public int Id;

        [JsonProperty()]
        public string Name;

        // Name in other languages other then default (when language="*")        
        [JsonProperty()]
        public List<LanguageContainer> NamesWithLanguages { get; set; }

        [JsonProperty()]
        public string Value;

        [JsonProperty()]
        public int? Threshold;
    }

    public class SegmentValues : SegmentBaseValue
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentSource Source;

        [JsonProperty()]
        public int? Threshold;

        [JsonProperty()]
        public List<SegmentValue> Values;
    }
}
