using ApiObjects.Json.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESIndexRebuildHandler
{
    public class IndexBuildRequest
    {
        [JsonProperty("group_id", Required = Required.Always)]
        public int GroupID { get; set; }

        [JsonProperty("switch_index_alias", Required = Required.Always)]
        public bool SwitchIndexAlias { get; set; }

        [JsonProperty("delete_old_indices", Required = Required.Always)]
        public bool DeleteOldIndices { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eObjectType Type { get; set; }

        [JsonProperty("start_date")]
        [JsonConverter(typeof(BaseTimeConverter))]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        [JsonConverter(typeof(BaseTimeConverter))]
        public DateTime? EndDate { get; set; }
    }
}
