using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetStruct : KalturaOTTObject
    {
        /// <summary>
        /// Asset Struct id 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Asset struct name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Asset Struct system name for the partner
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        ///  Is the Asset Struct protected by the system
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE, IsNullable = true)]
        public bool? IsProtected { get; set; }

        /// <summary>
        /// A list of comma separated meta ids associated with this asset struct, returned according to the order.
        /// </summary>
        [DataMember(Name = "metaIds")]
        [JsonProperty("metaIds")]
        [XmlElement(ElementName = "metaIds", IsNullable = true)]
        public string MetaIds { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset Struct last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// List of supported features
        /// </summary>
        [DataMember(Name = "features")]
        [JsonProperty("features")]
        [XmlElement(ElementName = "features", IsNullable = true)]
        public string Features { get; set; }

        /// <summary>
        /// Plural Name
        /// </summary>
        [DataMember(Name = "pluralName")]
        [JsonProperty("pluralName")]
        [XmlElement(ElementName = "pluralName", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string PluralName { get; set; }

        /// <summary>
        /// AssetStruct parent Id
        /// </summary>
        [DataMember(Name = "parentId")]
        [JsonProperty("parentId")]
        [XmlElement(ElementName = "parentId", IsNullable = true)]
        [SchemeProperty(MinLong = 0, IsNullable = true)]
        public long? ParentId { get; set; }

        /// <summary>
        /// connectingMetaId
        /// </summary>
        [DataMember(Name = "connectingMetaId")]
        [JsonProperty("connectingMetaId")]
        [XmlElement(ElementName = "connectingMetaId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? ConnectingMetaId { get; set; }

        /// <summary>
        /// connectedParentMetaId
        /// </summary>
        [DataMember(Name = "connectedParentMetaId")]
        [JsonProperty("connectedParentMetaId")]
        [XmlElement(ElementName = "connectedParentMetaId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? ConnectedParentMetaId { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlArray(ElementName = "dynamicData", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        [XmlArrayItem("item")]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }
    }
}