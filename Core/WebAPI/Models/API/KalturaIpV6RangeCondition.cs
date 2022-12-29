using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.Exceptions;

namespace WebAPI.Models.API
{
    /// <summary>
    /// IP V6 range condition
    /// </summary>
    [Serializable]
    public partial class KalturaIpV6RangeCondition : KalturaCondition
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
            this.Type = KalturaRuleConditionType.IP_V6_RANGE;
        }
    }
}