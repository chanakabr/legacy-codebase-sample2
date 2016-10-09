using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    [JsonObject]
    public class DMSGroupConfiguration
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("partner_id")]
        public int PartnerId { get; set; }

        [JsonProperty("is_default")]
        public bool IsDefault { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("number_of_devices")]
        public long NumberOfDevices { get; set; }

        [JsonProperty("config_files")]
        public List<DMSConfigurationMin> ConfigFileIds { get; set; }

        [JsonProperty("type")]
        private string docType { get; set; }

        public DMSGroupConfiguration()
        {
            this.docType = "group_configuration";
            this.Tags = new List<string>();
            this.ConfigFileIds = new List<DMSConfigurationMin>();
        }
    }
}
