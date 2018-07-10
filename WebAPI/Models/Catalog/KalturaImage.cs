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
        [SchemeProperty(MinInteger = 1)]
        public long ImageTypeId { get; set; }

        /// <summary>
        /// ID of the object the image is related to
        /// </summary>
        [DataMember(Name = "imageObjectId")]
        [JsonProperty(PropertyName = "imageObjectId")]
        [XmlElement(ElementName = "imageObjectId")]
        [SchemeProperty(MinInteger = 1)]
        public long ImageObjectId { get; set; }

        /// <summary>
        /// Type of the object the image is related to
        /// </summary>
        [DataMember(Name = "imageObjectType")]
        [JsonProperty(PropertyName = "imageObjectType")]
        [XmlElement(ElementName = "imageObjectType")]
        public KalturaImageObjectType? ImageObjectType { get; set; }

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
        /// Image content ID
        /// </summary>
        [DataMember(Name = "contentId")]
        [JsonProperty(PropertyName = "contentId")]
        [XmlElement(ElementName = "contentId")]
        [SchemeProperty(ReadOnly = true)]
        public string ContentId { get; set; }

        /// <summary>
        ///  Specifies if the image is default for atleast one image type.
        /// </summary>
        [DataMember(Name = "isDefault")]
        [JsonProperty("isDefault")]
        [XmlElement(ElementName = "isDefault", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsDefault { get; set; }
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