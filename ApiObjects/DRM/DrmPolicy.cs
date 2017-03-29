using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.DRM
{
    [Serializable]
    public class DrmPolicy
    {
        [JsonProperty("policy")]
        public DrmSecurityPolicy Policy { get; set; }

        [JsonProperty("familyLimitation")]
        public List<int> FamilyLimitation { get; set; }

        public DrmPolicy()
        {
            Policy = DrmSecurityPolicy.DeviceLevel;
            FamilyLimitation = new List<int>();
        }
    }
}
