using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaImage : KalturaOTTObject
    {
        /// <summary>
        /// Image ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly =true)]
        public long Id { get; set; }

        /// <summary>
        /// Image version 
        /// </summary>
        [DataMember(Name = "version")]
        [JsonProperty(PropertyName = "version")]
        [XmlElement(ElementName = "version")]
        [SchemeProperty(ReadOnly = true)]
        public string Version { get; set; }

        /// <summary>
        /// Image type ID
        /// </summary>
        [DataMember(Name = "imageTypeId")]
        [JsonProperty(PropertyName = "imageTypeId")]
        [XmlElement(ElementName = "imageTypeId")]
        public long ImageTypeId { get; set; }

        /// <summary>
        /// ID of the object the image is related to
        /// </summary>
        [DataMember(Name = "imageObjectId")]
        [JsonProperty(PropertyName = "imageObjectId")]
        [XmlElement(ElementName = "imageObjectId")]
        public long ImageObjectId { get; set; }

        /// <summary>
        /// Type of the object the image is related to
        /// </summary>
        [DataMember(Name = "imageObjectType")]
        [JsonProperty(PropertyName = "imageObjectType")]
        [XmlElement(ElementName = "imageObjectType")]
        public KalturaImageObjectType ImageObjectType { get; set; }

        /// <summary>
        /// Image content status 
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaImageStatus Status { get; set; }

        /// <summary>
        /// Image URL
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty(PropertyName = "url")]
        [XmlElement(ElementName = "url")]
        [SchemeProperty(ReadOnly = true)]
        public string Url { get; set; }

        /// <summary>
        /// Image system name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty(PropertyName = "systemName")]
        [XmlElement(ElementName = "systemName")]
        [SchemeProperty(ReadOnly = true)]
        public string SystemName { get; set; }
    }

    public class KalturaImageListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of images
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaImage> Images { get; set; }
    }
}