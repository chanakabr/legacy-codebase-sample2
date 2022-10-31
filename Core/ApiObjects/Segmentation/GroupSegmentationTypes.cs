using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiObjects.Segmentation
{
    public class GroupSegmentationTypes
    {
        [JsonProperty(PropertyName = "segmentationTypes")]
        public List<long> segmentationTypes;
    }
}