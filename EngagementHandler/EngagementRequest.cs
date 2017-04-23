using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EngagementHandler
{
    public class EngagementRequest
    {
        [JsonProperty("group_id")]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("start_time")]
        public int StartTime
        {
            get;
            set;
        }

        [JsonProperty("engagement_id")]
        public int EngagementId
        {
            get;
            set;
        }

        [JsonProperty("engagement_bulk_id")]
        public string EngagementBulkId
        {
            get;
            set;
        }
    }
}
