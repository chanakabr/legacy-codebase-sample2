using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// IP range condition
    /// </summary>
    [Serializable]
    public partial class KalturaIpRangeCondition : KalturaCondition
    {
        /// <summary>
        /// From IP address range
        /// </summary>
        [DataMember(Name = "fromIP")]
        [JsonProperty("fromIP")]
        [XmlElement(ElementName = "fromIP")]
        public string FromIP { get; set; }

        /// <summary>
        /// TO IP address range
        /// </summary>
        [DataMember(Name = "toIP")]
        [JsonProperty("toIP")]
        [XmlElement(ElementName = "toIP")]
        public string ToIP { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.IP_RANGE;
        }
    }
}