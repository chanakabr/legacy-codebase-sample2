using Newtonsoft.Json;

namespace WebAPI.Models.DMS
{
    [JsonObject]    
    public class DMSTagMapping
    {
        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonProperty("partner_id")]
        public int PartnerId { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("type")]
        private string docType { get; set; }

        public DMSTagMapping()
        {
            this.docType = "tag_map";
        }
    }
}
