using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SlimAssetInfo
    {
        [JsonProperty(PropertyName = "id")]
        public long Id
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "type")]
        public int Type
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get;
            set;
        }

    }
}
