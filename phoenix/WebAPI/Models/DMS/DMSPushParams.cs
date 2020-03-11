using Newtonsoft.Json;

namespace WebAPI.Models.DMS
{
    public class DMSPushParams
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("external_token")]
        public string ExternalToken { get; set; }
    }
}
