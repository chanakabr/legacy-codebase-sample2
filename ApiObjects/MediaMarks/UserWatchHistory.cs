using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    public class UserWatchHistory
    {
        [JsonProperty("uid")]
        public int UserID { get; set; }

        [JsonProperty("mid")]
        public int MediaId { get; set; }

        [JsonProperty("loc")]
        public int Location { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("ts")]
        public DateTime LastWatch { get; set; }

        [JsonProperty("pt")]
        public ePlayType PlayType { get; set; }

        [JsonProperty("mtid")]
        public int MediaTypeId { get; set; }
    }
}
