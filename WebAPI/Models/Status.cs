using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    [DataContract]
    public class StatusWrapper
    {
        public StatusWrapper(StatusCode code, object result = null, string msg = null)
        {
            if (code == StatusCode.OK)
                msg = "success";

            Status = new Status((int)code, msg);
            Result = result;
        }
        
        [DataMember(EmitDefaultValue = false, Name = "result")]
        public object Result { get; set; }

        [DataMember(Name = "status")]
        public Status Status { get; set; }
    }

    [DataContract]
    public class Status
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        public Status(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public Status()
        {
        }
    }

    public enum StatusCode
    {
        OK = 0,
        Error = 1,

        // 500000 - 599999 - TVPAPI Statuses
        BadCredentials = 500000,
        InternalConnectionIssue = 500001,
        Timeout = 500002,
        BadRequest = 500003
    }
}