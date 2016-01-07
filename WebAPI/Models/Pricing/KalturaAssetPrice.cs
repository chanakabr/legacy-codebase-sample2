using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public class KalturaAssetPrice : KalturaOTTObject
    {
        /// <summary>
        /// Asset identifier  
        /// </summary>
        [DataMember(Name = "asset_id")]
        [JsonProperty("asset_id")]
        [XmlElement(ElementName = "asset_id")]
        public string AssetId
        {
            get;
            set;
        }

        /// <summary>
        /// Asset type  
        /// </summary>
        [DataMember(Name = "asset_type")]
        [JsonProperty("asset_type")]
        [XmlElement(ElementName = "asset_type")]
        public KalturaAssetType AssetType
        {
            get;
            set;
        }

        /// <summary>
        /// Files and their prices
        /// </summary>
        [DataMember(Name = "file_prices")]
        [JsonProperty("file_prices")]
        [XmlArray(ElementName = "file_prices", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaItemPrice> FilePrices
        {
            get;
            set;
        }
    }
}