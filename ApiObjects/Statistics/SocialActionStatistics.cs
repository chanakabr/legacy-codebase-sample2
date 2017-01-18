using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Statistics
{
    [Serializable]
    [JsonObject(Id = "social_action")]
    public class SocialActionStatistics
    {
        [JsonProperty("media_id")]
        public int MediaID { get; set; }
        [JsonProperty("group_id")]
        public int GroupID { get; set; }
        [JsonProperty("media_type")]
        public string MediaType { get; set; }
        [JsonConverter(typeof(ApiObjects.JsonSerializers.BaseTimeConverter))]
        [JsonProperty("action_date")]
        public DateTime Date { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        [JsonProperty("rate_value", NullValueHandling=NullValueHandling.Ignore)]
        public int RateValue { get; set; }

        public SocialActionStatistics()
        {
            GroupID = 0;
            MediaType = string.Empty;
            Date = DateTime.UtcNow;
            Action = string.Empty;
            RateValue = 0;
        }
    }
}
