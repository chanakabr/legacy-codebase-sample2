using ApiObjects;
using ApiObjects.JsonSerializers;
using EventBus.Abstraction;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class SetupTaskRequest : ServiceEvent
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

        public override string ToString()
        {
            return $"{{{nameof(GroupID)}={GroupID}, {nameof(Mission)}={Mission}, {nameof(DynamicData)}={DynamicData}, {nameof(GroupId)}={GroupId}," +
                $" {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
