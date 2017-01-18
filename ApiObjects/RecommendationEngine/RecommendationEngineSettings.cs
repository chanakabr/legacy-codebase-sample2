using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    [JsonObject()]
    public class RecommendationEngineSettings
    {
        [DataMember]
        [JsonProperty()]
        public string key
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty()]
        public string value
        {
            get;
            set;
        }

        public RecommendationEngineSettings()
        {

        }

        public RecommendationEngineSettings(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

    }
}
