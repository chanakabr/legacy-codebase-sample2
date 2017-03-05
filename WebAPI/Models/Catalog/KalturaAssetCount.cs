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
        public List<KalturaAssetsCount> Objects
        {
            get;
            set;
        }
    }

    public class KalturaAssetCount : KalturaOTTObject
    {
        /// <summary>
        /// Value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        [XmlElement(ElementName = "value")]
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Count
        /// </summary>
        [DataMember(Name = "count")]
        [JsonProperty(PropertyName = "count")]
        [XmlElement(ElementName = "count")]
        public string Count
        {
            get;
            set;
        }

        /// <summary>
        /// Sub counts
        /// </summary>
        [DataMember(Name = "subs")]
        [JsonProperty(PropertyName = "subs")]
        [XmlArray(ElementName = "subs", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetsCount> SubCounts
        {
            get;
            set;
        }
    }

    public class KalturaAssetsCount : KalturaOTTObject
    {
        /// <summary>
        /// Field
        /// </summary>
        [DataMember(Name = "field")]
        [JsonProperty(PropertyName = "field")]
        [XmlElement(ElementName = "field")]
        public string Field
        {
            get;
            set;
        }

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
}