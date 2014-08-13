using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Epg
{
    [JsonObject(Title = "Mate")]
    public class Meta
    {
        [JsonProperty("MetaType")]
        public string MetaType { get; set; }
        
        [JsonProperty("MetaValues")]
        public List<MetaValues> MetaValues { get; set; } 
    }


    [JsonObject(Title = "MetaValues")]
    public class MetaValues
    {       
        [JsonProperty("Language")]
        public string Language { get; set; }

        [JsonProperty("MetaValue")] // meta value specific language
        public string MetaValue { get; set; }

        [JsonProperty("MetaValueMain")]   // meta value by main language
        public string MetaValueMain { get; set; }

    }



}
