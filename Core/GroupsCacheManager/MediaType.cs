using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GroupsCacheManager
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "media_type")]
    public class MediaType
    {
        [DataMember]
        public int id;
        [DataMember]
        public string name;
        [DataMember]
        public string description;
        [DataMember]
        public bool isLinear;
        [DataMember]
        public int parentId;
        [DataMember]
        public string associationTag;
    }
}
