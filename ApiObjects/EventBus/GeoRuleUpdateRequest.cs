using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class GeoRuleUpdateRequest : ServiceEvent
    {
        [JsonProperty("asset_rule_id")]
        public long AssetRuleId { get; set; }

        [JsonProperty("countries_to_remove")]
        public List<int> CountriesToRemove { get; set; }

        [JsonProperty("remove_blocked")]
        public bool RemoveBlocked { get; set; }

        [JsonProperty("remove_allowed")]
        public bool RemoveAllowed { get; set; }

        [JsonProperty("update_ksql")]
        public bool UpdateKsql { get; set; }
    }
}
