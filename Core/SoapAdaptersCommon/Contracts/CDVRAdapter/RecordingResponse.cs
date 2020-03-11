using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace CDVRAdapter.Models
{
    [DataContract]
    public class RecordingResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }

        [DataMember]
        public Recording Recording { get; set; }
    }
}