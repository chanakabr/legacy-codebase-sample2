using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class EntryJSON
    {
        [JsonProperty("assetId")]
        public string AssetID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("protected")]
        public bool Protected { get; set; }

        [JsonProperty("alreadyWatched")]
        public bool AlreadyWatched { get; set; }

        [JsonProperty("eventId")]
        public string EventID { get; set; }

        [JsonProperty("channelId")]
        public string ChannelID { get; set; }

        [JsonProperty("channelName")]
        public string ChannelName { get; set; }

        [JsonProperty("endTime", Required = Required.Default)]
        public string EndTime { get; set; }

        [JsonProperty("actualStartTime", Required = Required.Default)]
        public string ActualStartTime { get; set; }

        [JsonProperty("actualEndTime", Required = Required.Default)]
        public string ActualEndTime { get; set; }

        [JsonProperty("bookingTime", Required = Required.Default)]
        public string BookingTime { get; set; }

        [JsonProperty("programId", Required = Required.Default)]
        public string ProgramID { get; set; }

        [JsonProperty("source", Required = Required.Default)]
        public string Source { get; set; }

        [JsonProperty("genre", Required = Required.Default)]
        public string Genre { get; set; }

        [JsonProperty("rating", Required = Required.Default)]
        public string Rating { get; set; }

        [JsonProperty("seasonId", Required = Required.Default)]
        public string SeasonID { get; set; }

        [JsonProperty("seasonName", Required = Required.Default)]
        public string SeasonName { get; set; }

        [JsonProperty("episode", Required = Required.Default)]
        public string Episode { get; set; }

        [JsonProperty("year", Required = Required.Default)]
        public string Year { get; set; }

        [JsonProperty("thumbnail", Required = Required.Default)]
        public string Thumbnail { get; set; }

        [JsonProperty("seasonNumber", Required = Required.Default)]
        public string SeasonNumber { get; set; }

        [JsonProperty("seriesId", Required = Required.Default)]
        public string SeriesId { get; set; }

        [JsonProperty("type", Required = Required.Default)]
        public string Type { get; set; }
    }
}
