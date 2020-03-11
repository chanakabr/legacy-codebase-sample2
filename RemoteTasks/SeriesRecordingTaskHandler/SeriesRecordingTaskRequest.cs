using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeriesRecordingTaskHandler
{
    [Serializable]
    public class SeriesRecordingTaskRequest
    {

        [JsonProperty("group_id", Required = Required.Always)]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("user_id", Required = Required.Always)]
        public string UserId
        {
            get;
            set;
        }

        [JsonProperty("domain_id", Required = Required.Always)]
        public long DomainId
        {
            get;
            set;
        }

        [JsonProperty("channel_id", Required = Required.Always)]
        public string ChannelId
        {
            get;
            set;
        }

        [JsonProperty("series_id", Required = Required.Always)]
        public string SeriesId
        {
            get;
            set;
        }

        [JsonProperty("season_number", Required = Required.Always)]
        public int SeasonNumber
        {
            get;
            set;
        }

        [JsonProperty("series_recording_task_type", Required = Required.Always)]
        public eSeriesRecordingTask SeriesRecordingTaskType
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("UserId: {0}, ", !string.IsNullOrEmpty(UserId) ? UserId.ToString() : ""));
            sb.Append(string.Format("DomainId: {0}, ", DomainId));
            sb.Append(string.Format("ChannelId: {0}, ", !string.IsNullOrEmpty(ChannelId) ? ChannelId.ToString() : ""));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));
            sb.Append(string.Format("SeriesId: {0}, ", !string.IsNullOrEmpty(SeriesId) ? SeriesId.ToString() : ""));
            sb.Append(string.Format("SeasonNumber: {0}, ", SeasonNumber));
            sb.Append(string.Format("SeriesRecordingTaskType: {0}, ", SeriesRecordingTaskType.ToString()));

            return sb.ToString();
        }

    }
}
