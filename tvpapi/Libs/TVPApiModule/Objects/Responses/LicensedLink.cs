using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class LicensedLink
    {
        [JsonProperty(PropertyName = "main_url")]  
        public string MainUrl { get; set; }

        [JsonProperty(PropertyName = "alternate_url")]  
        public string AlternateUrl { get; set; }
    }
}
