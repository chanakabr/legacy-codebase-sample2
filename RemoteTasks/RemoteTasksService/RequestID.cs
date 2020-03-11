using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RemoteTasksService
{
    [Serializable]
    public class RequestID
    {
        [JsonProperty("req_id")]
        public string RequestId { get; set; }
    }
}