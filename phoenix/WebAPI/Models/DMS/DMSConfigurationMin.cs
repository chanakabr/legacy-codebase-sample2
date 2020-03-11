using Newtonsoft.Json;

namespace WebAPI.Models.DMS
{
    [JsonObject]
    public class DMSConfigurationMin
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}