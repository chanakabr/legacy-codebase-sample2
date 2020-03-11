using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Ingest.Models
{
    [DataContract]
    public class Status
    {
        [DataMember]
        public int Code { get; set; }

        [DataMember]
        public string Message { get; set; }

        public Status(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}