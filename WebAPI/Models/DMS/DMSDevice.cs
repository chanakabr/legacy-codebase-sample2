using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WebAPI.Models.DMS
{
    public class BaseReport
    {
    }

    [JsonObject]
    public class DMSDevice : BaseReport
    {
        [JsonProperty("partner_id")]
        public int GroupId { get; set; }

        [JsonProperty("udid")]
        public string Udid { get; set; }

        [JsonProperty("push")]
        public DMSPushParams PushParameters { get; set; }

        [JsonProperty("version_number")]
        public string VersionNumber { get; set; }

        [JsonProperty("version_platform")]
        [JsonConverter(typeof(StringEnumConverter))]
        public DMSePlatform VersionPlatform { get; set; }

        [JsonProperty("version_app_name")]
        public string VersionAppName { get; set; }

        [JsonProperty("last_access_ip")]
        public string LastAccessIP { get; set; }

        [JsonProperty("last_access_date")]
        public long LastAccessDate { get; set; }

        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }

        [JsonProperty("operation_system")]
        public string OperationSystem { get; set; }

        [JsonProperty("group_configuration_id")]
        public string GroupConfigurationId { get; set; }

        [JsonProperty("type")]
        private string docType { get; set; }

        public DMSDevice()
        {
            this.docType = "device";
        }
    }
}
