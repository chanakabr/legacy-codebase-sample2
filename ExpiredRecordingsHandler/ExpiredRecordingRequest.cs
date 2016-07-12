using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpiredRecordingsHandler
{
    [Serializable]
    public class ExpiredRecordingRequest
    {

        [JsonProperty("group_id", Required = Required.Always)]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty("id", Required = Required.Always)]
        public long Id
        {
            get;
            set;
        }

        [JsonProperty("recording_id", Required = Required.Always)]
        public long RecordingId
        {
            get;
            set;
        }

        [JsonProperty("recording_expiration_epoch", Required = Required.Always)]
        public long RecordingExpirationEpoch
        {
            get;
            set;
        }

        [JsonProperty("recording_expiration_date", Required = Required.Always)]
        public DateTime RecordingExpirationDate
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("RecordingId: {0}, ", RecordingId));
            sb.Append(string.Format("GroupId: {0}, ", GroupId));
            sb.Append(string.Format("RecordingExpirationDate: {0}, ", RecordingExpirationDate != null ? RecordingExpirationDate.ToString() : ""));
            sb.Append(string.Format("RecordingExpirationEpoch: {0}, ", RecordingExpirationEpoch));

            return sb.ToString();
        }

    }
}