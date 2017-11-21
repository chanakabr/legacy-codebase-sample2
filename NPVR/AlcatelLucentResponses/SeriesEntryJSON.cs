using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class SeriesEntryJSON
    {
        [JsonProperty("seasonId")]
        public string SeriesID { get; set; }

        [JsonProperty("seasonName")]
        public string SeriesName { get; set; }

        [JsonProperty("id")]
        public string RecordingID { get; set; }

        [JsonProperty("channelId")]
        public string ChannelID { get; set; }

        [JsonProperty("seasonNumber")]
        public string seasonNumber { get; set; }

        [JsonProperty("seasonId")]
        public string seasonId { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }
    }
}
