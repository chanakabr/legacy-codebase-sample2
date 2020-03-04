using Newtonsoft.Json;

namespace MailChimp.Campaigns
{
    public class Recipients
    {
        [JsonProperty("list_id")]
        public string ListId { get; set; }
    }
}
