using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace TVPApiModule.Objects
{
    public class ClientResponseStatus
    {
        public ClientResponseStatus()
        {
            Status = new Status();
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
