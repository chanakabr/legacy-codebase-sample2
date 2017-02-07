using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    [Serializable]
    public class LayeredCacheGroupConfig
    {
        [JsonProperty("GroupId")]
        public int GroupId { get; set; }

        [JsonProperty("Version")]
        public string Version { get; set; }

        [JsonProperty("DisableLayeredCache")]
        public bool DisableLayeredCache { get; set; }

        [JsonProperty("LayeredCacheSettingsToExclude")]
        public HashSet<string> LayeredCacheSettingsToExclude { get; set; }

        public LayeredCacheGroupConfig()
        {
            GroupId = 0;
            Version = string.Empty;
            LayeredCacheSettingsToExclude = new HashSet<string>();
            DisableLayeredCache = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("LayeredCacheGroupConfig for groupId: {0}, ", GroupId));
            sb.AppendFormat("Version: {0}, ", string.IsNullOrEmpty(Version) ? string.Empty : Version);            
            sb.AppendFormat("DisableLayeredCache: {0}, ", DisableLayeredCache.ToString());
            sb.AppendFormat("LayeredCacheSettingsToExclude: {0} ", string.Join(",", LayeredCacheSettingsToExclude));

            return sb.ToString();
        }

    }
}
