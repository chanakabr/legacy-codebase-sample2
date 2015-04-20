using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects
{
    public class Group
    {
        [JsonProperty(PropertyName = "advertising_values_metas")]
        public List<string> AdvertisingValuesMetas { get; set; }

        [JsonProperty(PropertyName = "advertising_values_tags")]
        public List<string> AdvertisingValuesTags { get; set; }

        [JsonProperty(PropertyName = "use_start_date")]
        public bool UseStartDate { get; set; }

        [JsonProperty(PropertyName = "should_support_single_login")]
        public bool ShouldSupportSingleLogin { get; set; }

        [JsonProperty(PropertyName = "should_support_friendly_url")]
        public bool ShouldSupportFriendlyURL { get; set; }

        [JsonIgnore]
        public object Languages { get; set; }
    }
}