using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ApiObjects.Json.Converters;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class ElasticSearchRequest : ServiceEvent
    {
        [JsonProperty("ids")]
        public List<long> DocumentIDs
        {
            get;
            set;
        }

        [JsonProperty("action", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eAction Action
        {
            get;
            set;
        }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eObjectType Type
        {
            get;
            set;
        }


        [JsonProperty("switch_index_alias")]
        public bool? SwitchIndexAlias
        {
            get;
            set;
        }

        [JsonProperty("delete_old_indices")]
        public bool? DeleteOldIndices
        {
            get;
            set;
        }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(BaseTimeConverter))]
        public DateTime? StartDate
        {
            get;
            set;
        }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(BaseTimeConverter))]
        public DateTime? EndDate
        {
            get;
            set;
        }
    }
}
