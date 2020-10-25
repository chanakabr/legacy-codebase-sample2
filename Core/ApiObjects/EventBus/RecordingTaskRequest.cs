using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class RecordingTaskRequest : DelayedServiceEvent
    {
        [JsonProperty("recording_task")]
        public eRecordingTask Task
        {
            get;
            set;
        }

        [JsonProperty("program_id")]
        public long ProgramId
        {
            get;
            set;
        }

        [JsonProperty("recording_id")]
        public long RecordingId
        {
            get;
            set;
        }

        [JsonProperty("epg_start_date")]
        public DateTime EpgStartDate
        {
            get;
            set;
        }

        [JsonProperty("max_domain_series_id")]
        public long? MaxDomainSeriesId
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{{{nameof(Task)}={Task}, {nameof(ProgramId)}={ProgramId}, {nameof(RecordingId)}={RecordingId}, {nameof(EpgStartDate)}={EpgStartDate}, {nameof(MaxDomainSeriesId)}={MaxDomainSeriesId}, {nameof(ETA)}={ETA}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
