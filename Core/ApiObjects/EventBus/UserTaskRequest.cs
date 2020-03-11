using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class UserTaskRequest : ServiceEvent
    {
        [JsonProperty("task")]
        public UserTaskType Task { get; set; }
        
        [JsonProperty("domain_id")]
        public int DomainId { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(Task)}={Task}, {nameof(DomainId)}={DomainId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
