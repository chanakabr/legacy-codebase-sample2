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

        public PlayCycleSession(int mediaConcurrencyRuleID, string playCycleKey, long createDateMs)
        {
            MediaConcurrencyRuleID = mediaConcurrencyRuleID;
            PlayCycleKey = playCycleKey;
            CreateDateMs = createDateMs;
        }
    }
}
