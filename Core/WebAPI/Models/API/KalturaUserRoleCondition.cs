using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// UserRole Condition - indicates which users this rule is applied on by their roles
    /// </summary>
    public partial class KalturaUserRoleCondition : KalturaCondition
    {
        /// <summary>
        /// Comma separated user role IDs list
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        protected override void Init()
        {
            base.Init();
            this.Type = KalturaRuleConditionType.USER_ROLE;
        }
    }
}