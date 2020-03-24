using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TVPApiModule.Objects.Responses
{
    public class ClientResponseStatus
    {
        public ClientResponseStatus()
        {
            Status = new Status();
        }

        public ClientResponseStatus(Status status)
        {
            Status = status;
        }

        public ClientResponseStatus(int code, string message)
        {
            Status = new Status();
            Status.Code = code;
            Status.Message = message;
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
