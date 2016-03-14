using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public PlayCycleSession(int mediaConcurrencyRuleID, string playCycleKey, long createDateMs, int domainID)
        {
            MediaConcurrencyRuleID = mediaConcurrencyRuleID;
            PlayCycleKey = playCycleKey;
            CreateDateMs = createDateMs;
            DomainID = domainID;
        }
    }
}
