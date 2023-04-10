using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess.FilterActions
{
    [Serializable]
    [SchemeClass(Required = new[] { "shopAssetUserRuleId" })]
    public partial class KalturaShopPreActionCondition : KalturaBasePreActionCondition
    {
        /// <summary>
        /// Asset user rule ID with shop condition
        /// </summary>
        [DataMember(Name = "shopAssetUserRuleId")]
        [JsonProperty("shopAssetUserRuleId")]
        [XmlElement(ElementName = "shopAssetUserRuleId")]
        [SchemeProperty(MinInteger = 1)]
        public int ShopAssetUserRuleId { get; set; }
    }
}