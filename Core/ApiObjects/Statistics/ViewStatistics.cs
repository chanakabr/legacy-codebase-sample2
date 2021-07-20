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
    public class MediaView : SocialActionStatistics
    {
        [JsonProperty("location")]
        public int Location { get; set; }
        
        public MediaView() : base()
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
