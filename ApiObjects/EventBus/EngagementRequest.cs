using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus
{
    public class EngagementRequest : ServiceEvent
    {
        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("engagement_id")]
        public int EngagementId { get; set; }

        [JsonProperty("engagement_bulk_id")]
        public int EngagementBulkId { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(StartTime)}={StartTime}, {nameof(EngagementId)}={EngagementId}, {nameof(EngagementBulkId)}={EngagementBulkId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
