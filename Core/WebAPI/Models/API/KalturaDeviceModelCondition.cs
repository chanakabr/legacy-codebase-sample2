using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    public partial class KalturaDeviceModelCondition : KalturaCondition
    {
        /// <summary>
        /// regex of device model that is compared to
        /// </summary>
        [DataMember(Name = "regexEqual")]
        [JsonProperty("regexEqual")]
        [XmlElement(ElementName = "regexEqual")]
        public string RegexEqual { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_MODEL;
        }
    }
}