using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// User asset rule filter
    /// </summary>
    public class KalturaUserAssetRuleFilter : KalturaFilter<KalturaUserAssetRuleOrderBy>
    {
        /// <summary>
        /// Asset identifier to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long? AssetIdEqual{ get; set; }

        /// <summary>
        /// Asset type to filter by - 0 = EPG, 1 = media
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public int? AssetTypeEqual { get; set; }

        internal long getAssetId()
        {
            return AssetIdEqual.HasValue ? (long)AssetIdEqual : 0;
        }

        public override KalturaUserAssetRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaUserAssetRuleOrderBy.NAME_ASC;
        }
    }
}