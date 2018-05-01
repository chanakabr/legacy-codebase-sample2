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
    /// Asset statistics
    /// </summary>
    public class KalturaAssetStructMeta : KalturaOTTObject
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
        [SchemeProperty(MaxLength = 255)]
        public string IngestReferencePath { get; set; }

        /// <summary>
        /// ProtectFromIngest
        /// </summary>
        [DataMember(Name = "protectFromIngest")]
        [JsonProperty(PropertyName = "protectFromIngest")]
        [XmlElement(ElementName = "protectFromIngest", IsNullable = true)]
        public bool? ProtectFromIngest { get; set; }

        /// <summary>
        /// DefaultIngestValue
        /// </summary>
        [DataMember(Name = "defaultIngestValue")]
        [JsonProperty(PropertyName = "defaultIngestValue")]
        [XmlElement(ElementName = "defaultIngestValue")]
        [SchemeProperty(MaxLength = 4000)]
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
    }
}