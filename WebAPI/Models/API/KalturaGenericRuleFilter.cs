using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Generic rule filter
    /// </summary>
    [Obsolete]
    public class KalturaGenericRuleFilter : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier to filter by
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement(ElementName = "assetId")]
        [OldStandardProperty("asset_id")]
        public long? AssetId{ get; set; }

        /// <summary>
        /// Asset type to filter by - 0 = EPG, 1 = media
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement(ElementName = "assetType")]
        [OldStandardProperty("asset_type")]
        public int? AssetType { get; set; }

        internal long getAssetId()
        {
            return AssetId.HasValue ? (long)AssetId : 0;
        }
    }
}