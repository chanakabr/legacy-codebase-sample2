using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.DRM
{
    public class Policy
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
