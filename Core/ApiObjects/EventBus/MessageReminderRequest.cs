using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class MessageReminderRequest : DelayedServiceEvent
    {
        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_reminder_id")]
        public int MessageReminderId { get; set; }

        public override string ToString()
        {
            return $"StartTime:{StartTime}, MessageReminderId:{MessageReminderId}";
        }
    }
}
