using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public class TvmDeviceRule : TvmRule
    {
        [JsonProperty("DeviceBrandIds")]
        public HashSet<int> DeviceBrandIds { get; set; }

        public TvmDeviceRule()
        {
            RuleType = TvmRuleType.Device;
        }
    }
}
