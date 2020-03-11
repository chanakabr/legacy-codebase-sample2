using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MessageInterestHandler
{
    public class MessageInterestRequest
    {
        [JsonProperty("group_id")]
        public int GroupId { get; set; }

        [JsonProperty("start_time")]
        public long StartTime { get; set; }

        [JsonProperty("message_interest_id")]
        public int MessageInterestId { get; set; }
    }
}
