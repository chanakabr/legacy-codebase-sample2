using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    public partial class KalturaUdidDynamicListCondition : KalturaCondition
    {
        /// <summary>
        /// KalturaUdidDynamicList.id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(DynamicMinInt = 1)]
        public long Id { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_UDID_DYNAMIC_LIST;
        }
    }
}