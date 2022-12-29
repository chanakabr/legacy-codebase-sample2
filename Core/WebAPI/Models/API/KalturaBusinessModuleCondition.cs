using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Business module condition
    /// </summary>
    public partial class KalturaBusinessModuleCondition : KalturaCondition
    {
        /// <summary>
        /// Business module type  
        /// </summary>
        [DataMember(Name = "businessModuleType")]
        [JsonProperty("businessModuleType")]
        [XmlElement(ElementName = "businessModuleType", IsNullable = true)]
        public KalturaTransactionType? BusinessModuleType { get; set; }

        /// <summary>
        /// Business module ID  
        /// </summary>
        [DataMember(Name = "businessModuleId")]
        [JsonProperty("businessModuleId")]
        [XmlElement(ElementName = "businessModuleId", IsNullable = true)]
        public long? BusinessModuleId { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.BUSINESS_MODULE;
        }
    }
}