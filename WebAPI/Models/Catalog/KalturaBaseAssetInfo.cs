using Jil;
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
    /// <summary>
    /// Slim assets wrapper
    /// </summary>
    public class KalturaSlimAssetInfoWrapper : KalturaListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaBaseAssetInfo> Objects { get; set; }
    }

    /// <summary>
    /// Slim asset info
    /// </summary>
    [OldStandard("mediaFiles", "media_files")]
    public class KalturaBaseAssetInfo : KalturaOTTObject, KalturaIAssetable
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Identifies the asset type (EPG, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public int? Type { get; set; }

        /// <summary>
        /// Asset name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Asset description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Collection of images details that can be used to represent this asset
        /// </summary>
        [DataMember(Name = "images", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "images", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaImage> Images { get; set; }

        /// <summary>
        /// Files
        /// </summary>
        [DataMember(Name = "mediaFiles", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "mediaFiles", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "mediaFiles", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaFile> MediaFiles { get; set; }

        /// <summary>
        /// Collection of add-on statistical information for the media. See AssetStats model for more information
        /// </summary>
        [DataMember(Name = "stats", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "stats", IsNullable = true)]
        [JilDirectiveAttribute(Ignore = true)]
        [Obsolete]
        public KalturaAssetStatistics Statistics { get; set; }


        internal int getType()
        {
            return Type.HasValue ? (int)Type : 0;
        }
    }
}