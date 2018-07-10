using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetStructMetaOrderBy
    {
        NONE
    }

    /// <summary>
    /// Filtering Asset Struct Metas
    /// </summary>
    [Serializable]
    public class KalturaAssetStructMetaFilter : KalturaFilter<KalturaAssetStructMetaOrderBy>
    {
        /// <summary>
        /// Filter Asset Struct metas that contain a specific asset struct id
        /// </summary>
        [DataMember(Name = "assetStructIdEqual")]
        [JsonProperty("assetStructIdEqual")]
        [XmlElement(ElementName = "assetStructIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? AssetStructIdEqual { get; set; }
        
        /// <summary>
        /// Filter Asset Struct metas that contain a specific meta id
        /// </summary>
        [DataMember(Name = "metaIdEqual")]
        [JsonProperty("metaIdEqual")]
        [XmlElement(ElementName = "metaIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? MetaIdEqual { get; set; }
        
        public override KalturaAssetStructMetaOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetStructMetaOrderBy.NONE;
        }

        internal void Validate()
        {
            if (!AssetStructIdEqual.HasValue && !MetaIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "AssetStructIdEqual", "MetaIdEqual");
            }
        }        
    }
}