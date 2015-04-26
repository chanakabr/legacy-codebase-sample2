using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class AutocompleteResponse
    {
        [JsonProperty(PropertyName = "assets")]
        public List<SlimAssetInfo> Assets
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
    }
}
