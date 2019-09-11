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

    }
}