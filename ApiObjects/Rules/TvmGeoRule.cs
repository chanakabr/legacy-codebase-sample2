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

        [JsonProperty("ProxyRuleId")]
        public int ProxyRuleId { get; set; }
        
        [JsonProperty("ProxyRuleName")]
        public string ProxyRuleName { get; set; }

        [JsonProperty("ProxyLevelId")]
        public int ProxyLevelId { get; set; }

        [JsonProperty("ProxyLevelName")]
        public string ProxyLevelName { get; set; }

        public TvmGeoRule()
        {
            TvmRuleType = RuleType.Geo;
        }
    }
}
