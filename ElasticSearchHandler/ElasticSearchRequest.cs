using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ApiObjects.Json.Converters;

namespace ElasticSearchHandler
{
    [Serializable]
    public class ElasticSearchRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("ids")]
        public List<int> DocumentIDs
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

        [JsonProperty("date")]
        [JsonConverter(typeof(BaseTimeConverter))]
        public DateTime? Date
        {
            get;
            set;
        }
        
    }
}
