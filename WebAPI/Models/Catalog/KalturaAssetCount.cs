using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public class KalturaAssetCountListResponse : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetCount> Objects
        {
            get;
            set;
        }
    }

    public class KalturaAssetCount : KalturaOTTObject
    {
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }

        [DataMember(Name = "count")]
        [JsonProperty(PropertyName = "count")]
        [XmlElement(ElementName = "count")]
        public string Count
        {
            get;
            set;
        }
    }
}