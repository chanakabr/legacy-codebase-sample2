using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;

namespace UserTaskHandler
{
    public class UserTaskRequest
    {
        [JsonProperty("group_id")]
        public int GroupID { get; set; }

        [JsonProperty("task")]
        public UserTaskType Task { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("domain_id")]
        public int DomainId { get; set; }
    }
}
