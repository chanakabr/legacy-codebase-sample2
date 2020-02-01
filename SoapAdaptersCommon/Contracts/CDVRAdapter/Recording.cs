using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDVRAdapter.Models
{
    [DataContract]
    public class Recording
    {
        [DataMember]
        public string RecordingId { get; set; }

        [DataMember]
        public int RecordingState { get; set; }

        [DataMember]
        public int FailReason { get; set; }

        [DataMember]
        public List<RecordingLink> Links { get; set; }

        [DataMember]
        public string ProviderStatusCode { get; set; }

        [DataMember]
        public string ProviderStatusMessage { get; set; }

        [DataMember]
        public List<long> FailedDomainIds { get; set; }
    }
}