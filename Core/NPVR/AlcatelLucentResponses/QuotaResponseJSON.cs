using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class QuotaResponseJSON
    {
        [JsonProperty("quota")]
        public long TotalQuota { get; set; }

        [JsonProperty("occupied")]
        public long OccupiedQuota { get; set; }
    }
}
