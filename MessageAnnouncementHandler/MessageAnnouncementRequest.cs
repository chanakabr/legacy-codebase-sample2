using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageAnnouncementHandler
{
    [Serializable]
    public class MessageAnnouncementRequest
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_announcement_id")]
        public int MessageAnnouncementId { get; set; }
    }
}
