using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public partial class KalturaApplyDiscountModuleAction : KalturaBusinessModuleRuleAction
    {
        /// <summary>
        /// Discount module ID
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        public long DiscountModuleId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleActionType.APPLY_DISCOUNT_MODULE;
        }
    }
}