using EventBus.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class FreeAssetUpdateRequest : DelayedServiceEvent
    {
        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eObjectType type
        {
            get;
            set;
        }

        [JsonProperty("asset_ids", Required = Required.Always)]
        public List<long> asset_ids
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{{{nameof(type)}={type}, {nameof(asset_ids)}={string.Join(",", asset_ids)}, {nameof(ETA)}={ETA}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}