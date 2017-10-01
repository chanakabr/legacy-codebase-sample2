using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Billing
{
    [Serializable]
    [JsonObject]
    public class UnifiedBillingCycle
    {
        [JsonProperty]
        public long endDate { get; set; }
    }
}
