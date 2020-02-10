using Newtonsoft.Json;

namespace MailChimpAdapter.Models
{
    public class AdapterConfiguration
    {
        public long AdapterId { get; set; }
        public int PartnerId { get; set; }
        public string SecretKey { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("from_email_address")]
        public string FromEmailAddress { get; set; }

        [JsonProperty("contact_company")]
        public string ContactCompany { get; set; }

        [JsonProperty("contact_address1")]
        public string ContactAddress1 { get; set; }

        [JsonProperty("contact_city")]
        public string ContactCity { get; set; }

        [JsonProperty("contact_state")]
        public string ContactState { get; set; }

        [JsonProperty("contact_zip")]
        public string ContactZip { get; set; }

        [JsonProperty("contact_country")]
        public string ContactCountry { get; set; }

        [JsonProperty("permission_reminder")]
        public string PermissionReminder { get; set; }
        
    }
}