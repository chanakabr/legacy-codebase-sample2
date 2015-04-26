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
        public StatusWrapper(StatusCode code, Guid reqID, object result = null, string msg = null)
        {
            Status = new Status((int)code, msg, reqID);
            Result = result;
        }

        [DataMember(Name = "result")]
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

        [DataMember(Name = "request_id")]
        public string RequestID { get; set; }

        public Status(int code, string message, Guid reqID)
        {
            Code = code;
            Message = message;
            RequestID = reqID.ToString();
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