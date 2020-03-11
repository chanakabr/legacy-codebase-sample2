using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class RegionsResponse
    {
        [JsonProperty(PropertyName = "regions")]
        public List<Region> Regions { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
