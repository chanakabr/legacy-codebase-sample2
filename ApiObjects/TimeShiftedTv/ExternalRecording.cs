using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ApiObjects.TimeShiftedTv
{

    public class ExternalRecording : Recording
    {
        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string ExternalDomainRecordingId { get; set; }

        public Dictionary<string, string> MetaData { get; set; }

        public ExternalRecording()
            : base()
        {
            this.isExternalRecording = true;
            this.ExternalDomainRecordingId = string.Empty;
        }

        public ExternalRecording(ExternalRecording externalRecording, string externalDomainRecordingId)
            : base(externalRecording)
        {            
            this.ExternalDomainRecordingId = externalDomainRecordingId;
            if (externalRecording.MetaData != null)
                this.MetaData = new Dictionary<string, string>(externalRecording.MetaData);
        }

        public ExternalRecording(ExternalRecording externalRecording)
            : base(externalRecording)
        {
            this.ExternalDomainRecordingId = externalRecording.ExternalDomainRecordingId;
            if (externalRecording.MetaData != null)
                this.MetaData = new Dictionary<string, string>(externalRecording.MetaData);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format("ExternalDomainRecordingId: {0}", string.IsNullOrEmpty(ExternalDomainRecordingId) ? string.Empty : ExternalDomainRecordingId));

            return sb.ToString();
        }

        public string MetaDataAsJson
        {
            get
            {
                try
                {
                    return JsonConvert.SerializeObject(this.MetaData, Formatting.None);
                }
                catch (Exception e)
                {
                    _log.Warn("error when trying to serialize MetaData to json");
                    return string.Empty;
                }
            }
        }
    }
}
