using ApiObjects;
using ApiObjects.JsonSerializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SegmentationTaskHandler
{
    [Serializable]
    public class SegmentationTaskRequest
    {
        [JsonProperty("group_id")]
        public int GroupID
        {
            get;
            set;
        }

        [JsonProperty("task_type")]
        public SegmentationTaskType TaskType
        {
            get;
            set;
        }

        [JsonProperty("users_segments")]
        public Dictionary<string, List<long>> UsersSegments
        {
            get;
            set;
        }
    }

    public enum SegmentationTaskType
    {
        update_user_segments
    }
}