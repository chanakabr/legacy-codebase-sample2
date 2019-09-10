using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ApiObjects
{
    [Serializable]
    public class RegionsCache
    {
        #region Data Members
        [JsonProperty()]
        public Dictionary<int, Region> Regions { get; set; }

        [JsonProperty()]
        public Dictionary<string, int> ExternalIdsMapping { get; set; }

        [JsonProperty()]
        public Dictionary<int, List<int>> ParentIdsToRegionIdsMapping { get; set; }

        #endregion

        #region Ctor

        public RegionsCache()
        {
            Regions = new Dictionary<int, Region>();
            ExternalIdsMapping = new Dictionary<string, int>();
            ParentIdsToRegionIdsMapping = new Dictionary<int, List<int>>();
        }

        #endregion
    }
}
