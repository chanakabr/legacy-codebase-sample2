using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Generic rule filter
    /// </summary>
    public class KalturaGenericRuleFilter : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier to filter by
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty("asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public long AssetId{ get; set; }

        /// <summary>
        /// Asset type to filter by - 0 = EPG, 1 = media
        /// </summary>
        [DataMember(Name = "asset_type")]
        [JsonProperty("asset_type")]
        [XmlElement(ElementName = "asset_type")]
        public int AssetType { get; set; }
    }
}