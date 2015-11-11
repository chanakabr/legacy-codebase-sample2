using ApiObjects;
using ApiObjects.JsonSerializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SetupTaskHandler
{
    [Serializable]
    public class SetupTaskRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("setup_task_type")]
        public eSetupTask? Mission
        {
            get;
            set;
        }

        [JsonProperty("dynamic_data")]
        public Dictionary<string, object> DynamicData
        {
            get;
            set;
        }
    }
}
