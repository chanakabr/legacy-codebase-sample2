using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    public partial class KalturaDynamicKeysCondition : KalturaCondition
    {
        /// <summary>
        /// key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        [SchemeProperty(MinLength = 1, Pattern = SchemeInputAttribute.ASCII_ONLY_PATTERN)]
        public string Key { get; set; }

        /// <summary>
        /// comma-separated values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlElement(ElementName = "values")]
        [SchemeProperty(MinLength = 1)]
        public string Values { get; set; }

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.DYNAMIC_KEYS;
        }
    }
}