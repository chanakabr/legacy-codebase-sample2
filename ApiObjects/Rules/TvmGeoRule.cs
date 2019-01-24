using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public class TvmGeoRule : TvmRule
    {
        [JsonProperty("OnlyOrBut")]
        public bool OnlyOrBut { get; set; }

        [JsonProperty("CountryIds")]
        public HashSet<int> CountryIds { get; set; }

        // TODO SHIR - DONT FORGET TO UPDATE TYPE AND MAPPING
        [JsonProperty("ProxyRule")]
        public int ProxyRule { get; set; }

        // TODO SHIR - DONT FORGET TO UPDATE TYPE AND MAPPING
        [JsonProperty("ProxyLevel")]
        public int ProxyLevel { get; set; }

        public TvmGeoRule()
        {
            TvmRuleType = RuleType.Geo;
        }
    }
}
