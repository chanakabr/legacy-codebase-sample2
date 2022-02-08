using System;
using Newtonsoft.Json;

namespace RemoteTasksService
{
    [Serializable]
    public class RequestParams
    {
        [JsonProperty("req_id")]
        public string RequestId { get; set; }

        [JsonProperty("group_id")]
        public string GroupId { get; set; }
    }
}