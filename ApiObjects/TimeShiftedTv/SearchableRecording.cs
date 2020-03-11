using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SearchableRecording
    {
        public string DomainRecordingId { get; set; }

        public long RecordingId { get; set; }

        public long EpgId { get; set; }

        public RecordingType? RecordingType { get; set; }

        public SearchableRecording() { }

        public SearchableRecording(long domainRecordingId, long recordingId, long epgId, RecordingType recordingType)
        {
            this.DomainRecordingId = domainRecordingId.ToString();
            this.RecordingId = recordingId;
            this.EpgId = epgId;
            this.RecordingType = recordingType;
        }

    }
}
