using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;

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

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.ASSET_SHOP;
        }

        public override void Validate(HashSet<KalturaRuleConditionType> types = null)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "value");
            }
        }
    }
}