using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Response
{
    public class StatusWrapper
    {
        public StatusWrapper()
        {
            Status = new Status();
        }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
    }
}
