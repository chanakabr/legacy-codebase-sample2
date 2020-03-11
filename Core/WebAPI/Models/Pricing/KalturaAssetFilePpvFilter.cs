using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public enum KalturaAssetFilePpvOrderBy
    {
        NONE
    }

    /// <summary>
    /// Filtering Asset Struct Metas
    /// </summary>
    [Serializable]
    public partial class KalturaAssetFilePpvFilter : KalturaFilter<KalturaAssetFilePpvOrderBy>
    {
        /// <summary>
        /// Filter Asset file ppvs that contain a specific asset id
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? AssetIdEqual { get; set; }

        /// <summary>
        /// Filter Asset file ppvs that contain a specific asset file id
        /// </summary>
        [DataMember(Name = "assetFileIdEqual")]
        [JsonProperty("assetFileIdEqual")]
        [XmlElement(ElementName = "assetFileIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? AssetFileIdEqual { get; set; }
        
        public override KalturaAssetFilePpvOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetFilePpvOrderBy.NONE;
        }

        internal void Validate()
        {
            if (!AssetFileIdEqual.HasValue && !AssetIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "AssetFileIdEqual, AssetIdEqual");
            }

            if (AssetFileIdEqual.HasValue && AssetIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetFilePpvFilter.AssetFileIdEqual", "KalturaAssetFilePpvFilter.AssetIdEqual");
            }
        }        
    }
}