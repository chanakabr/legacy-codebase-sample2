using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Response;
using Newtonsoft.Json;

namespace ApiObjects
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
            Status = new Status(code, message);
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
