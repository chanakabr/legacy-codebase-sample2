using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset-file type
    /// </summary>
    public class KalturaAssetFileType : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public int? Id { get; set; }

        /// <summary>
        /// Unique name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        [SchemeProperty(MinLength = 1, MaxLength = 50)]
        public string Name { get; set; }

        /// <summary>
        /// Unique description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        [SchemeProperty(MinLength = 1, MaxLength = 50)]
        public string Description { get; set; }

        /// <summary>
        /// Indicates if asset-file type is active or disabled
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public bool? Status { get; set; }

        /// <summary>
        /// Specifies when was the type was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the type last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? UpdateDate { get; set; }

        /// <summary>
        /// Specifies whether playback as trailer is allowed
        /// </summary>
        [DataMember(Name = "isTrailer")]
        [JsonProperty(PropertyName = "isTrailer")]
        [XmlElement(ElementName = "isTrailer")]
        [SchemeProperty(InsertOnly = true)]
        public bool? IsTrailer { get; set; }

        /// <summary>
        /// Defines playback streamer type
        /// </summary>
        [DataMember(Name = "streamerType")]
        [JsonProperty(PropertyName = "streamerType")]
        [XmlElement(ElementName = "streamerType")]
        [SchemeProperty(InsertOnly = true)]
        public KalturaAssetFileStreamerType? StreamerType { get; set; }

        /// <summary>
        /// DRM adapter-profile identifier, use -1 for uDRM, 0 for no DRM.
        /// </summary>
        [DataMember(Name = "drmProfileId")]
        [JsonProperty(PropertyName = "drmProfileId")]
        [XmlElement(ElementName = "drmProfileId")]
        [SchemeProperty(MinInteger = -1)]
        public int DrmProfileId { get; set; }

        /// <summary>
        ///  Asset file type quality
        /// </summary>
        [DataMember(Name = "quality")]
        [JsonProperty("quality")]
        [XmlElement(ElementName = "quality", IsNullable = true)]        
        public KalturaAssetFileTypeQuality? Quality { get; set; }

        public void validateForInsert()
        {
            if (Name == null || Name.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetFileType.name");
            }

            if (StreamerType == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetFileType.streamerType");
            }
        }
    }

    /// <summary>
    /// Asset-file types list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public class KalturaAssetFileTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of asset-file types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetFileType> Types { get; set; }
    }
    
    public enum KalturaAssetFileStreamerType
    {
        APPLE_HTTP = 0,
        MPEG_DASH = 1,
        URL = 2,
        SMOOTH_STREAMING = 3
    }
}