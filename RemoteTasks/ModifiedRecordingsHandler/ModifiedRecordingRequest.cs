using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifiedRecordingsHandler
{
    [Serializable]
    public class ModifiedRecordingRequest
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

        [JsonProperty("scheduled_expiration_epoch", Required = Required.Always)]
        public long ScheduledExpirationEpoch
        {
            get;
            set;
        }

        [JsonProperty("old_recording_duration", Required = Required.Always)]
        public int OldRecordingDuration
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
            sb.Append(string.Format("ScheduledExpirationEpoch: {0}, ", ScheduledExpirationEpoch));
            sb.Append(string.Format("OldRecordingDuration: {0}, ", OldRecordingDuration));

            return sb.ToString();
        }

    }
}