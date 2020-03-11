using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    [Serializable]
    public class DeviceConcurrencyPriority
    {
        [JsonProperty("DeviceFamilyIds")]
        public List<int> DeviceFamilyIds { get; set; }

        [JsonProperty("PriorityOrder")]
        public DowngradePolicy PriorityOrder { get; set; }
    }
}
