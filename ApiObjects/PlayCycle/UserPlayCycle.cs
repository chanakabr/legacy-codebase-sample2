using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// NEW
using ODBCWrapper;
using ApiObjects;
using ApiObjects.ConditionalAccess;
using ApiObjects.Catalog;
using ApiObjects.Rules;

namespace ApiObjects.PlayCycle
{
    [Serializable]
    public class PlayCycleSession
    {
        [JsonProperty("MediaConcurrencyRuleID")]
        public int MediaConcurrencyRuleID { get; set; }

        [JsonProperty("PlayCycleKey")]
        public string PlayCycleKey { get; set; }

        [JsonProperty("CreateDateMs")]
        public long CreateDateMs { get; set; }

        [JsonProperty("DomainID")]
        public int DomainID { get; set; }

        [JsonProperty("MediaConcurrencyRuleIds")]
        public List<int> MediaConcurrencyRuleIds { get; set; }
        
        [JsonProperty("AssetMediaRuleIds")]
        public List<long> AssetMediaRuleIds { get; set; }

        [JsonProperty("AssetEpgRuleIds")]
        public List<long> AssetEpgRuleIds { get; set; }

        public PlayCycleSession(int mediaConcurrencyRuleID, string playCycleKey, long createDateMs, int domainID, List<int> mediaConcurrencyRuleIds, List<long> assetMediaRuleIds, List<long> assetEpgRuleIds)
        {
            this.MediaConcurrencyRuleID = mediaConcurrencyRuleID;
            this.PlayCycleKey = playCycleKey;
            this.CreateDateMs = createDateMs;
            this.DomainID = domainID;
            this.MediaConcurrencyRuleIds = mediaConcurrencyRuleIds;
            this.AssetMediaRuleIds = assetMediaRuleIds;
            this.AssetEpgRuleIds = assetEpgRuleIds;
        }
    }
}
