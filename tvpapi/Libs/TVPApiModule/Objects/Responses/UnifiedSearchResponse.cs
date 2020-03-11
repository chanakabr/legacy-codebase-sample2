using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UnifiedSearchResponse
    {
        [JsonProperty(PropertyName = "assets")]
        public List<AssetInfo> Assets
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "total_items")]
        public int TotalItems
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "request_id", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId
        {
            get;
            set;
        }
    }
}
