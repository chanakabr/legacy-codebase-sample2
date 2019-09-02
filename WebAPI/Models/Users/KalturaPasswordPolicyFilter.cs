using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings filter
    /// </summary>
    public partial class KalturaPasswordPolicyFilter : KalturaCrudFilter<KalturaPasswordPolicyOrderBy, PasswordPolicy, long, PasswordPolicyFilter>
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

        public override ICrudHandler<PasswordPolicy, long, PasswordPolicyFilter> Handler
        {
            get
            {
                return PasswordPolicyManager.Instance;
            }
        }
        
        public override KalturaPasswordPolicyOrderBy GetDefaultOrderByValue()
        {
            return KalturaPasswordPolicyOrderBy.NONE;
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(UserRoleIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPasswordPolicyFilter.roleIdsIn");
            }
        }
    }

    public enum KalturaPasswordPolicyOrderBy
    {
        NONE
    }
}