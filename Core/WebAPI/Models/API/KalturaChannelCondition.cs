using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    [SchemeClass(Required = new string[] { "idIn" })]
    public partial class KalturaChannelCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated channel IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1, MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.CHANNEL;
        }
    }
}