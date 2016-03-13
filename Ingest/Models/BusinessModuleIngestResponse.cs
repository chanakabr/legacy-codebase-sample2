using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Ingest.Models
{
    [DataContract]
    public class BusinessModuleIngestResponse
    {
        [DataMember]
        public string ReportFilename { get; set; }

        [DataMember]
        public Status Status { get; set; }
    }
}