using Newtonsoft.Json;

namespace MailChimp.Campaigns
{
    public class Campaign
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("recipients")]
        public Recipients Recipients { get; set; }

        [JsonProperty("settings")]
        public Settings Settings { get; set; }
    }
}
