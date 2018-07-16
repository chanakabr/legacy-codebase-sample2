using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class ExternalRecording: Recording
    {

        public string ExternalDomainRecordingId { get; set; }

        public ExternalRecording()
            : base()
        {
            this.isExternalRecording = true;
            this.ExternalDomainRecordingId = string.Empty;
        }

        public ExternalRecording(Recording recording, string externalDomainRecordingId)
            : base(recording)
        {
            this.isExternalRecording = true;
            this.ExternalDomainRecordingId = externalDomainRecordingId;
        }

        public ExternalRecording(ExternalRecording externalRecording)
            : base(externalRecording)
        {
            this.isExternalRecording = true;
            this.ExternalDomainRecordingId = externalRecording.ExternalDomainRecordingId;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format("ExternalDomainRecordingId: {0}", string.IsNullOrEmpty(ExternalDomainRecordingId) ? string.Empty : ExternalDomainRecordingId));

            return sb.ToString();
        }

    }
}
