using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;
using System;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class AssetInheritanceRequest : ServiceEvent
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        [JsonProperty("type")]
        public InheritanceType? Type { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(GroupId)}={GroupId}, {nameof(Data)}={Data}, {nameof(UserId)}={UserId}, {nameof(Type)}={Type}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
