using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    public partial class KalturaDeviceFamilyCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated Device Family IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.DEVICE_FAMILY;
        }
    }
}