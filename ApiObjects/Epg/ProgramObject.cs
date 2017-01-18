using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Epg
{
    [JsonObject(Title = "ProgramObject")]
    public class ProgramObject
    {
        [JsonProperty("Identifier")]
        public string Identifier { get; set; }

        [JsonProperty("StartDate")]
        public string StartDate { get; set; }
        [JsonProperty("EndDate")]
        public string EndDate { get; set; }
        [JsonProperty("Pic")]
        public string Pic { get; set; }

        [JsonProperty("Name")]
        public List<KeyValuePair<string, string>> Name { get; set; } // Language, name

        [JsonProperty("Description")]
        public List<KeyValuePair<string, string>> Description { get; set; }

        [JsonProperty("lMetas")]
        public List<Meta> lMetas { get; set; }

        [JsonProperty("lTags")]
        public List<Tag> lTags { get; set; }
    }
}
