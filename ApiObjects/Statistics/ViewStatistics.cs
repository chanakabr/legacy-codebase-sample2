using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;

namespace ApiObjects.Statistics
{
    [Serializable]
    [JsonObject(Id = "view_stats")]
    public class ViewStatistics
    {
        [JsonProperty("count")]
        public long Counter { get; set; }

        public ViewStatistics()
        {
            Counter = 0;
        }

    }

    [Serializable]
    [JsonObject(Id = "media_view")]
    public class MediaView
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
        [JsonProperty("location")]
        public int Location { get; set; }

        public MediaView()
        {
            GroupID = 0;
            MediaType = string.Empty;
            Date = DateTime.UtcNow;
            Action = string.Empty;
            Location = 0;

        }
    }

    [Serializable]
    [JsonObject(Id = "StatisticsView")]
    public class StatisticsView : MediaView
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        public StatisticsView()
            : base()
        {
            ID = string.Empty;
        }
    }
}
