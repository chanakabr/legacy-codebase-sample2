using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class SegmentRanges : SegmentBaseValue
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentSource Source;
        
        [JsonProperty()]
        public List<SegmentRange> Ranges;
    }

    public class SegmentRange
    {
        [JsonProperty()]
        public string Name;

        [JsonProperty()]
        public List<LanguageContainer> NamesWithLanguages;

        [JsonProperty()]
        public double GreaterThanOrEquals;

        [JsonProperty()]
        public double GreaterThan;

        [JsonProperty()]
        public double LessThanOrEquals;

        [JsonProperty()]
        public double LessThan;
    }
}
