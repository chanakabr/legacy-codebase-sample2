using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset statistics
    /// </summary>
    public partial class KalturaAssetStructMeta : KalturaOTTObject
    {
        /// <summary>
        ///  Asset Struct id (template_id)
        /// </summary>
        [DataMember(Name = "assetStructId")]
        [JsonProperty(PropertyName = "assetStructId")]
        [XmlElement(ElementName = "assetStructId")]
        [SchemeProperty(ReadOnly = true)]
        public long AssetStructId { get; set; }

        /// <summary>
        /// Meta id (topic_id)
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty(PropertyName = "metaId")]
        [XmlElement(ElementName = "metaId")]
        [SchemeProperty(ReadOnly = true)]
        public long MetaId { get; set; }

        /// <summary>
        /// IngestReferencePath
        /// </summary>
        [DataMember(Name = "ingestReferencePath")]
        [JsonProperty(PropertyName = "ingestReferencePath")]
        [XmlElement(ElementName = "ingestReferencePath")]
        [SchemeProperty(MaxLength = 255, IsNullable = true)]
        public string IngestReferencePath { get; set; }

        /// <summary>
        /// ProtectFromIngest
        /// </summary>
        [DataMember(Name = "protectFromIngest")]
        [JsonProperty(PropertyName = "protectFromIngest")]
        [XmlElement(ElementName = "protectFromIngest", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? ProtectFromIngest { get; set; }

        /// <summary>
        /// DefaultIngestValue
        /// </summary>
        [DataMember(Name = "defaultIngestValue")]
        [JsonProperty(PropertyName = "defaultIngestValue")]
        [XmlElement(ElementName = "defaultIngestValue")]
        [SchemeProperty(MaxLength = 4000, IsNullable = true)]
        public string DefaultIngestValue { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct Meta was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct Meta last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Is inherited
        /// </summary>
        [DataMember(Name = "isInherited")]
        [JsonProperty(PropertyName = "isInherited")]
        [XmlElement(ElementName = "isInherited", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? IsInherited { get; set; }

        /// <summary>
        /// Is Location Tag
        /// </summary>
        [DataMember(Name = "isLocationTag")]
        [JsonProperty(PropertyName = "isLocationTag")]
        [XmlElement(ElementName = "isLocationTag", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? IsLocationTag { get; set; }

        /// <summary>
        /// suppressed Order, ascending
        /// </summary>
        [DataMember(Name = "suppressedOrder")]
        [JsonProperty(PropertyName = "suppressedOrder")]
        [XmlElement(ElementName = "suppressedOrder", IsNullable = true)]
        [SchemeProperty(MinInteger = 0, IsNullable = true)]
        public int? SuppressedOrder { get; set; }

        /// <summary>
        /// Case sensitive alias value
        /// </summary>
        [DataMember(Name = "aliasName")]
        [JsonProperty("aliasName")]
        [XmlElement(ElementName = "aliasName", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MaxLength = 50)]
        public string AliasName { get; set; }
    }
}