using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MessageReminderHandler
{
    [Serializable]
    public class MessageReminderRequest
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_reminder_id")]
        public int MessageReminderId { get; set; }
    }
}
