using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "regionId")]
    public class Region
    {
        #region Data Members
        [JsonProperty()]
        public int id;
        [JsonProperty()]
        public string name;
        [JsonProperty()]
        public string externalId;
        [JsonProperty()]
        public bool isDefault;
        [JsonProperty()]
        public List<KeyValuePair> linearChannels;
        [JsonProperty()]
        public int groupId;

        #endregion

        #region Ctor

        public Region()
        {
            name = string.Empty;
            externalId = string.Empty;
            isDefault = false;
            linearChannels = new List<KeyValuePair>();
        }

        #endregion
    }
}
