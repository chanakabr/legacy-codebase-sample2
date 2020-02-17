using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings filter
    /// </summary>
    public partial class KalturaPasswordPolicyFilter : KalturaCrudFilter<KalturaPasswordPolicyOrderBy, PasswordPolicy>
    {
        /// <summary>
        /// Comma separated list of role Ids.
        /// </summary>
        [DataMember(Name = "userRoleIdIn")]
        [JsonProperty("userRoleIdIn")]
        [XmlElement(ElementName = "userRoleIdIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string UserRoleIdIn { get; set; }

        public KalturaPasswordPolicyFilter() : base()
        {
        }

        public override KalturaPasswordPolicyOrderBy GetDefaultOrderByValue()
        {
            return KalturaPasswordPolicyOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public override GenericListResponse<PasswordPolicy> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<PasswordPolicyFilter>(this);
            return PasswordPolicyManager.Instance.List(contextData, coreFilter);
        }
    }

    public enum KalturaPasswordPolicyOrderBy
    {
        NONE
    }
}