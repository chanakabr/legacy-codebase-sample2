using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    [Serializable]
    [JsonObject()]
    public class AssetsPartialUpdate
    {
        [DataMember()]
        [JsonProperty()]
        public List<int> AssetIds { get; set; }
        [DataMember()]
        [JsonProperty()]
        public eObjectType AssetType { get; set; }
        [DataMember()]
        [JsonProperty()]
        public List<PartialUpdate> Updates { get; set; }
    }

    [Serializable]
    [JsonObject()]
    public class PartialUpdate
    {

        [DataMember()]
        [JsonProperty()]
        public string LanguageCode { get; set; }
        [DataMember()]
        [JsonProperty()]
        public bool ShouldUpdateAllLanguages { get; set; }
        [DataMember()]
        [JsonProperty()]
        public string FieldName { get; set; }
        [DataMember()]
        [JsonProperty()]
        public string OriginalValue { get; set; }
        [DataMember()]
        [JsonProperty()]
        public string NewValue { get; set; }
        [DataMember()]
        [JsonProperty()]
        public eUpdateFieldType FieldType { get; set; }
        [DataMember()]
        [JsonProperty()]
        public eUpdateFieldAction Action { get; set; }
    }

    public enum eUpdateFieldType
    {
        Basic,
        Tag,
        Meta
    }

    public enum eUpdateFieldAction
    {
        Update,
        Replace,
        Delete
    }
}
