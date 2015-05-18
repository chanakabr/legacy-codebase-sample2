using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TVPApiModule.Objects.Responses
{
    public class Status
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
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

    public enum eStatus
    {
        OK = 0,
        Error = 1,

        // 500000 - 599999 - TVPAPI Statuses
        BadCredentials = 500000,
        InternalConnectionIssue = 500001,
        Timeout = 500002,
        BadRequest = 500003,
        Unauthorized = 500004
    }
}
