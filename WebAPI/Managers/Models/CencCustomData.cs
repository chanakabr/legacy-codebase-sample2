using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Models
{
    public class CencCustomData
    {
        [JsonProperty("ca_system")]
        public string CaSystem { get; set; }

        [JsonProperty("account_id")]
        public int AccountId { get; set; }

        [JsonProperty("content_id")]
        public string ContentId { get; set; }

        [JsonProperty("files")]
        public string Files { get; set; }

        [JsonProperty("user_token")]
        public string UserToken { get; set; }

        [JsonProperty("udid")]
        public string UDID { get; set; }

        [JsonProperty("additional_cas_system")]
        public int AdditionalCasSystem { get; set; }
    }
}