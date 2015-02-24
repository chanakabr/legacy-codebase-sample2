using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Response;
using Newtonsoft.Json;

namespace ApiObjects
{
    class ClientResponseStatus
    {
        public ClientResponseStatus()
        {
            Status = new Status();
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
