using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using System.Collections.Generic;
using System;
using System.Linq;
using WebAPI.Exceptions;
using ApiObjects.Base;
using ApiObjects.Response;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings
    /// </summary>
    [Serializable]
    public partial class KalturaPasswordPolicy : KalturaCrudObject<PasswordPolicy, long>
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(MinLength = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Comma separated UserRole Ids list which the policy is applied on
        /// </summary>
        [DataMember(Name = "userRoleIds")]
        [JsonProperty("userRoleIds")]
        [XmlElement(ElementName = "userRoleIds")]
        [SchemeProperty(DynamicMinInt = 0, MinLength = 1)]
        public string UserRoleIds { get; set; }

        /// <summary>
        /// The number of passwords that should be remembered for each user so that they cannot be reused.
        /// </summary>
        [DataMember(Name = "historyCount")]
        [JsonProperty("historyCount")]
        [XmlElement(ElementName = "historyCount", IsNullable = true)]
        public int? HistoryCount { get; set; }

        /// <summary>
        /// When should the password expire (will represent time as days).
        /// </summary>
        [DataMember(Name = "expiration")]
        [JsonProperty("expiration")]
        [XmlElement(ElementName = "expiration", IsNullable = true)]
        public int? Expiration { get; set; }

        /// <summary>
        /// array of  KalturaRegex
        /// </summary>
        [DataMember(Name = "complexities")]
        [JsonProperty("complexities")]
        [XmlElement(ElementName = "complexities", IsNullable = true)]
        public List<KalturaRegexExpression> Complexities { get; set; }

        /// <summary>
        ///  the number of passwords failures before the account is locked.
        /// </summary>
        [DataMember(Name = "lockoutFailuresCount")]
        [JsonProperty("lockoutFailuresCount")]
        [XmlElement(ElementName = "lockoutFailuresCount", IsNullable = true)]
        public int? LockoutFailuresCount { get; set; }

        public KalturaPasswordPolicy() : base() { }

        internal override ICrudHandler<PasswordPolicy, long> Handler
        {
            get
            {
                return PasswordPolicyManager.Instance;
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        internal override void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(this.UserRoleIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "userRoleIds");
            }
            this.ValidateComplexities();
        }

        internal override void ValidateForUpdate()
        {
            this.ValidateComplexities();
        }

        internal void ValidateComplexities()
        {
            if (this.Complexities?.Count > 0)
            {
                foreach (var pattern in this.Complexities)
                {
                    pattern.Validate();
                }
            }
        }

        internal override GenericResponse<PasswordPolicy> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<PasswordPolicy>(this);
            return PasswordPolicyManager.Instance.Add(contextData, coreObject);
        }

        internal override GenericResponse<PasswordPolicy> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<PasswordPolicy>(this);
            return PasswordPolicyManager.Instance.Update(contextData, coreObject);
        }
    }

    public partial class KalturaPasswordPolicyListResponse : KalturaListResponse<KalturaPasswordPolicy>
    {
        public KalturaPasswordPolicyListResponse() : base() { }
    }
}