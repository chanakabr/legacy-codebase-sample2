using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Media-file type
    /// </summary>
    public partial class KalturaMediaFileType : KalturaOTTObject
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
        [SchemeProperty(MinLength = 1, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COLONS_PATTERN)]
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
        /// Indicates if media-file type is active or disabled
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(IsNullable = true)]
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
        [SchemeProperty(InsertOnly = true, IsNullable = true)]
        public bool? IsTrailer { get; set; }

        /// <summary>
        /// Defines playback streamer type
        /// </summary>
        [DataMember(Name = "streamerType")]
        [JsonProperty(PropertyName = "streamerType")]
        [XmlElement(ElementName = "streamerType")]
        [SchemeProperty(InsertOnly = true)]
        public KalturaMediaFileStreamerType? StreamerType { get; set; }

        /// <summary>
        /// DRM adapter-profile identifier, use -1 for uDRM, 0 for no DRM.
        /// </summary>
        [DataMember(Name = "drmProfileId")]
        [JsonProperty(PropertyName = "drmProfileId")]
        [XmlElement(ElementName = "drmProfileId")]
        [SchemeProperty(MinInteger = -1, InsertOnly = true)]
        public int DrmProfileId { get; set; }

        /// <summary>
        ///  Media file type quality
        /// </summary>
        [DataMember(Name = "quality")]
        [JsonProperty("quality")]
        [XmlElement(ElementName = "quality", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaMediaFileTypeQuality? Quality { get; set; }

        /// <summary>
        /// List of comma separated video codecs
        /// </summary>
        [DataMember(Name = "videoCodecs")]
        [JsonProperty("videoCodecs")]
        [XmlElement(ElementName = "videoCodecs", IsNullable = true)]
        [SchemeProperty(MaxLength = 100, IsNullable = true)]
        public string VideoCodecs { get; set; }

        /// <summary>
        /// List of comma separated audio codecs
        /// </summary>
        [DataMember(Name = "audioCodecs")]
        [JsonProperty("audioCodecs")]
        [XmlElement(ElementName = "audioCodecs", IsNullable = true)]
        [SchemeProperty(MaxLength = 100, IsNullable = true)]
        public string AudioCodecs { get; set; }

        /// <summary>
        /// List of comma separated keys allowed to be used as KalturaMediaFile's dynamic data keys
        /// </summary>
        [DataMember(Name = "dynamicDataKeys")]
        [JsonProperty("dynamicDataKeys")]
        [XmlElement(ElementName = "dynamicDataKeys", IsNullable = true)]
        public string DynamicDataKeys { get; set; }
    }
}