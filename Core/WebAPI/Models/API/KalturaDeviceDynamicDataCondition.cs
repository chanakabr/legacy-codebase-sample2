using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    public partial class KalturaDeviceDynamicDataCondition : KalturaCondition
    {
        /// <summary>
        /// key
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        [SchemeProperty(Pattern = SchemeInputAttribute.NOT_EMPTY_PATTERN)]
        public string Key { get; set; }

        /// <summary>
        /// value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        [SchemeProperty(Pattern = SchemeInputAttribute.NOT_EMPTY_PATTERN)]
        public string Value { get; set; }

        protected override void Init()
        {
            base.Init();
            Type = KalturaRuleConditionType.DEVICE_DYNAMIC_DATA;
        }
    }
}