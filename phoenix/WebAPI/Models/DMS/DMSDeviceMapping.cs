using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace WebAPI.Models.DMS
{
    [DataContract]
    public class DMSDeviceMapping
    {
        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonProperty("partner_id")]
        public int PartnerId { get; set; }

        [JsonProperty("udid")]
        public string Udid { get; set; }

        [JsonProperty("type")]
        private string docType { get; set; }

        public DMSDeviceMapping()
        {
            this.docType = "device_map";
        }
    }
}
