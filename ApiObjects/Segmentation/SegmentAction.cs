using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Segmentation
{
    public class SegmentAction
    {
    }

    public class SegmentAssetOrderAction : SegmentAction
    {
        [JsonProperty()]
        public string Name { get; set; }

        [JsonProperty()]
        public List<string> Values { get; set; }
    }
}
