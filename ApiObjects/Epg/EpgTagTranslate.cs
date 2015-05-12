using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    [Serializable]
    public class EpgTagTranslate
    {
        public EpgTagTranslate()
        {
        }


        public EpgTagTranslate(string language, string value, int id)
        {
           this.language = language;
            this.value = value;
            this.ID = id;
        }

        public EpgTagTranslate(string language, string value, string valueMain)
        {
            this.language = language;
            this.value = value;
            this.valueMain = valueMain;
        }

        [JsonProperty("language")]
        public string language { get; set; }
        [JsonProperty("value")]
        public string value { get; set; }
        [JsonProperty("id")]
        public int ID { get; set; }
        [JsonProperty("valueMain")]
        public string valueMain { get; set; }
    }
}

