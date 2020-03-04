using Newtonsoft.Json;

namespace MailChimp.Campaigns
{
    public class Settings
    {
        [JsonProperty("subject_line")]
        public string SubjectLine { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("from_name")]
        public string FromName { get; set; }

        [JsonProperty("reply_to")]
        public string ReplyTo { get; set; }

        [JsonProperty("template_id")]
        public int TemplateId { get; set; }
    }
}
