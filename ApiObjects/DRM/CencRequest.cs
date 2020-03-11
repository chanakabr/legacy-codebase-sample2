using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.DRM
{
    public class CencRequest
    {
        [JsonProperty("ca_system")]
        public string CaSystem { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("content_id")]
        public string ContentId { get; set; }

        [JsonProperty("files")]
        public string FileId { get; set; }

        [JsonProperty("policy")]
        public Policy Policy { get; set; }
    }
}
