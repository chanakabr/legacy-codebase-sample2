using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
    [Serializable]
    public class ParentalRulesTags
    {
        [JsonProperty()]
        [DataMember]
        public Dictionary<string, List<string>> mediaTags
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public Dictionary<string, List<string>> epgTags
        {
            get;
            set;
        }
    }
}
