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
        /// Indicates if media-file type is active or disabled
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
        public KalturaMediaFileTypeQuality? Quality { get; set; }

        /// <summary>
        /// List of comma separated video codecs
        /// </summary>
        [DataMember(Name = "videoCodecs")]
        [JsonProperty("videoCodecs")]
        [XmlElement(ElementName = "videoCodecs", IsNullable = true)]
        [SchemeProperty(MaxLength = 100)]
        public string VideoCodecs { get; set; }

        /// <summary>
        /// List of comma separated audio codecs
        /// </summary>
        [DataMember(Name = "audioCodecs")]
        [JsonProperty("audioCodecs")]
        [XmlElement(ElementName = "audioCodecs", IsNullable = true)]
        [SchemeProperty(MaxLength = 100)]
        public string AudioCodecs { get; set; }

        public void validateForInsert()
        {
            if (Name == null || Name.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "mediaFileType.name");
            }

            if (Description == null || Description.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "mediaFileType.description");
            }

            if (StreamerType == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "mediaFileType.streamerType");
            }
        }

        internal HashSet<string> CreateMappedHashSetForMediaFileType(string codecs)
        {
            HashSet<string> result = null;
            if (!string.IsNullOrEmpty(codecs))
            {
                string[] codecValues = codecs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (codecValues != null && codecValues.Length > 0)
                {
                    result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string codec in codecValues)
                    {
                        if (!result.Contains(codec))
                        {
                            result.Add(codec);
                        }
                    }
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Media-file types list
    /// </summary>
    [DataContract(Name = "Collections", Namespace = "")]
    [XmlRoot("Collections")]
    public partial class KalturaMediaFileTypeListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of media-file types
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaFileType> Types { get; set; }
    }
    
    public enum KalturaMediaFileStreamerType
    {
        APPLE_HTTP = 0,
        MPEG_DASH = 1,
        URL = 2,
        SMOOTH_STREAMING = 3
    }
}