using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDVRAdapter.Models
{
    public class RecordingLink
    {
        [DataMember]
        public string FileType { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}