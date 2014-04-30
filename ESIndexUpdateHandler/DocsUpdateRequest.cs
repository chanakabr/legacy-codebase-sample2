using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ESIndexUpdateHandler
{
    [Serializable]
    public class DocsUpdateRequest
    {
        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("ids")]
        public List<int> DocIDs { get; set; }

        [JsonProperty("action", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eAction Action { get; set; }

        [JsonProperty("type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.eObjectType Type { get; set; }

    }
}
