using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// UserSessionProfile Condition
    /// </summary>
    public partial class KalturaUserSessionProfileCondition : KalturaCondition
    {
        /// <summary>
        /// UserSessionProfile id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(MinLong = 1)]
        public long Id { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_SESSION_PROFILE;
        }
    }
}