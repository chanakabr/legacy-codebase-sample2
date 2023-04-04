using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess.FilterActions
{
    public abstract partial class KalturaFilterAction : KalturaAssetRuleAction
    {
        /// <summary>
        /// PreAction condition
        /// </summary>
        [DataMember(Name = "preActionCondition")]
        [JsonProperty("preActionCondition")]
        [XmlElement(ElementName = "preActionCondition", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaBasePreActionCondition PreActionCondition { get; set; }
    }
}
