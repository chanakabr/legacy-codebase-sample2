using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaRelatedEntity : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the related entry
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]        
        public string Id { get; set; }

        /// <summary>
        /// Defines related entry type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public KalturaRelatedEntityType Type { get; set; }
    }

    public enum KalturaRelatedEntityType
    {        
        CHANNEL = 0,
        EXTERNAL_CHANNEL = 1,
        MEDIA = 2,
        PROGRAM = 3
    }

    [Serializable]
    public partial class KalturaRelatedEntityArray : KalturaOTTObject
    {
        /// <summary>
        /// List of related entities
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaRelatedEntity> Objects { get; set; }

        protected override void Init()
        {
            base.Init();
            Objects = new List<KalturaRelatedEntity>();
        }
    }
}