using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        public List<KeyValuePair<long, int>> linearChannels;
        [JsonProperty()]
        public int groupId;
        [JsonProperty()]
        public int parentId;
        [JsonProperty()]
        public DateTime createDate;
        [JsonProperty()]
        public int childrenCount;

        #endregion

        #region Ctor

        public Region()
        {
            name = string.Empty;
            externalId = string.Empty;
            isDefault = false;
            linearChannels = new List<KeyValuePair<long, int>>();
        }

        #endregion
    }
}
