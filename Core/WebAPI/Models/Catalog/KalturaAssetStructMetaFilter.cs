using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Filtering Asset Struct Metas
    /// </summary>
    [Serializable]
    public partial class KalturaAssetStructMetaFilter : KalturaFilter<KalturaAssetStructMetaOrderBy>
    {
        /// <summary>
        /// Filter Asset Struct metas that contain a specific asset struct id
        /// </summary>
        [DataMember(Name = "assetStructIdEqual")]
        [JsonProperty("assetStructIdEqual")]
        [XmlElement(ElementName = "assetStructIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 0, IsNullable = true)]
        public long? AssetStructIdEqual { get; set; }
        
        /// <summary>
        /// Filter Asset Struct metas that contain a specific meta id
        /// </summary>
        [DataMember(Name = "metaIdEqual")]
        [JsonProperty("metaIdEqual")]
        [XmlElement(ElementName = "metaIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1, IsNullable = true)]
        public long? MetaIdEqual { get; set; }

        public override KalturaAssetStructMetaOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetStructMetaOrderBy.NONE;
        }
    }
}