using ApiObjects;
using Newtonsoft.Json;
using System;

namespace AssetInheritanceHandler
{
    [Serializable]
    public class AssetInheritanceRequest
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("type")]
        public InheritanceType? Type { get; set; }
    }
}
