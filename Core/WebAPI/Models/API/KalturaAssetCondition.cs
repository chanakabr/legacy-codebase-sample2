using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset Condition
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new []{"ksql"})]
    public partial class KalturaAssetCondition : KalturaAssetConditionBase
    {
        /// <summary>
        /// KSQL  
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty("ksql")]
        [XmlElement(ElementName = "ksql")]
        public string Ksql { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.ASSET;
        }
    }
}