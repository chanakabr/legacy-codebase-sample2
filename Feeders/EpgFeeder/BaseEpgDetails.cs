using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace EpgFeeder
{
     [JsonObject(Title = "BaseEpgDetails")]
    public class BaseEpgDetails
    {
        [JsonProperty("ChannelId")]
        public string ChannelId { get; set; }
        [JsonProperty("Identifier")]
        public string Identifier { get; set; }
        [JsonProperty("Name")]
        public string Name { get; set; }
        [JsonProperty("Description")]
        public string Description { get; set; }
        [JsonProperty("StartDate")]
        public string StartDate { get; set; }
        [JsonProperty("EndDate")]
        public string EndDate { get; set; }
        [JsonProperty("Pic")]
        public string Pic { get; set; }
        [JsonProperty("Language")]
        public string Language { get; set; }
    }
}
