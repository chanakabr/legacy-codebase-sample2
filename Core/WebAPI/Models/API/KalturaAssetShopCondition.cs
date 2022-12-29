using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaAssetShopCondition : KalturaAssetConditionBase
    {
        /// <summary>
        /// Shop marker's value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }

        /// <summary>
        /// Shop marker's values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlElement(ElementName = "values")]
        public KalturaStringValueArray Values { get; set; }

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.ASSET_SHOP;
        }
    }
}