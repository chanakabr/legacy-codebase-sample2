using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstFollowerRecordingHandler
{
    [Serializable]
    public class FirstFollowerRecordingRequest
    {

        [JsonProperty("group_id", Required = Required.Always)]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("domainId", Required = Required.Always)]
        public long DomainId
        {
            get;
            set;
        }

        [JsonProperty("channelId", Required = Required.Always)]
        public string ChannelId
        {
            get;
            set;
        }

        [JsonProperty("seriesId", Required = Required.Always)]
        public string SeriesId
        {
            get;
            set;
        }

        [JsonProperty("seasonNumber", Required = Required.Always)]
        public int SeasonNumber
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("DomainId: {0}, ", DomainId));
            sb.Append(string.Format("ChannelId: {0}, ", !string.IsNullOrEmpty(ChannelId) ? ChannelId.ToString() : ""));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));
            sb.Append(string.Format("SeriesId: {0}, ", !string.IsNullOrEmpty(SeriesId) ? SeriesId.ToString() : ""));
            sb.Append(string.Format("SeasonNumber: {0}, ", SeasonNumber));

            return sb.ToString();
        }

    }
}
