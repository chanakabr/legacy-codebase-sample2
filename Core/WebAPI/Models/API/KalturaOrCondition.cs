using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    [SchemeClass(Required = new string[] { "conditions" })]
    public partial class KalturaOrCondition : KalturaNotCondition
    {
        /// <summary>
        /// List of conditions with or between them  
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaCondition> Conditions { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.OR;
        }
    }
}