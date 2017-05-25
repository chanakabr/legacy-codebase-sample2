using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset meta
    /// </summary>
    public class KalturaMeta : KalturaOTTObject
    {
        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Meta system field name 
        /// </summary>
        [DataMember(Name = "fieldName")]
        [JsonProperty("fieldName")]
        [XmlElement(ElementName = "fieldName")]
        public KalturaMetaFieldName FieldName { get; set; }

        /// <summary>
        ///  Meta value type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaMetaType Type { get; set; }

        /// <summary>
        /// Asset type this meta is related to 
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// List of supported features 
        /// </summary>
        [DataMember(Name = "features")]
        [JsonProperty("features")]
        [XmlElement(ElementName = "features", IsNullable = true)]
        [SchemeProperty(DynamicType = typeof(KalturaMetaFeatureType))]
        public string Features { get; set; }

        /// <summary>
        /// Meta id 
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty("metaId")]
        [XmlElement(ElementName = "metaId")]
        public string MetaId { get; set; }

        /// <summary>
        /// Parent meta id
        /// </summary>
        [DataMember(Name = "parentMetaId")]
        [JsonProperty("parentMetaId")]
        [XmlElement(ElementName = "parentMetaId")]
        public string ParentMetaId{ get; set; }

        /// <summary>
        /// Partner Id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        public int PartnerId { get; set; }
    }

    [Serializable]
    public enum KalturaMetaFeatureType
    {
        USER_INTEREST,
        ENABLED_NOTIFICATION
    } 
}