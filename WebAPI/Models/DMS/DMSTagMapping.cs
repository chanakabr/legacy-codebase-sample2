using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebAPI.Models.DMS
{
    [DataContract]
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
