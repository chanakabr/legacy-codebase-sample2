using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.AssetPersonalMarkup
{
    /// <summary>
    /// Asset Personal Markup search filter
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "assetsIn" })]
    public partial class KalturaAssetPersonalMarkupSearchFilter : KalturaFilter<KalturaAssetPersonalMarkupSearchOrderBy>
    {
        /// <summary>
        /// all assets to search their personal markups
        /// </summary>
        [DataMember(Name = "assetsIn")]
        [JsonProperty("assetsIn")]
        [XmlArray(ElementName = "assetsIn")]
        [XmlArrayItem("item")]
        [SchemeProperty(MinItems = 1, MaxItems = 100)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public List<KalturaSlimAsset> AssetsIn { get; set; }

        public override KalturaAssetPersonalMarkupSearchOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetPersonalMarkupSearchOrderBy.NONE;
        }
    }
}