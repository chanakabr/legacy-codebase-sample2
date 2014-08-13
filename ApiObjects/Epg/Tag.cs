using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Epg
{
    [JsonObject(Title = "Tag")]
    public class Tag
    {
        [JsonProperty("TagType")]
        public string TagType { get; set; }

        [JsonProperty("TagValues")]
        public List<TagValues> TagValues { get; set; } 
    }

    [JsonObject(Title = "TagValues")]
    public class TagValues
    {   
        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("TagValue")] // tag value specific language
        public string TagValue { get; set; }

        [JsonProperty("TagValueMain")] // tag value by main language
        public string TagValueMain { get; set; }
    }

}
