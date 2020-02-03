using AdapaterCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDVRAdapter.Models
{
    [DataContract]
    public class ExternalRecordingResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }

        [DataMember]
        public List<CloudRecording> ExternalRecordings { get; set; }
    }
}