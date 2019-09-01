using ApiLogic.Base;
using ApiLogic.Users.Managers;
using ApiObjects;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings
    /// </summary>
    public partial class KalturaPasswordPolicy : KalturaCrudObject<PasswordPolicy, long, PasswordPolicyFilter>
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
        public string Name { get; set; }

        /// <summary>
        /// Comma separated UserRole Ids list which the policy is applied on
        /// </summary>
        [DataMember(Name = "appliedUserRoleIds")]
        [JsonProperty("appliedUserRoleIds")]
        [XmlElement(ElementName = "appliedUserRoleIds")]
        [SchemeProperty(DynamicMaxInt = 0)]
        public string AppliedUserRoleIds { get; set; }

        /// <summary>
        /// Minimum password length
        /// </summary>
        [DataMember(Name = "minimumLength")]
        [JsonProperty("minimumLength")]
        [XmlElement(ElementName = "minimumLength", IsNullable = true)]
        public int? MinimumLength { get; set; }

        /// <summary>
        /// The number of passwords that should be remembered for each user so that they cannot be reused.
        /// </summary>
        [DataMember(Name = "passwordsHistory")]
        [JsonProperty("passwordsHistory")]
        [XmlElement(ElementName = "passwordsHistory", IsNullable = true)]
        public int? PasswordsHistory { get; set; }

        /// <summary>
        /// When should the password expire (will represent time as days).
        /// </summary>
        [DataMember(Name = "passwordAge")]
        [JsonProperty("passwordAge")]
        [XmlElement(ElementName = "passwordAge", IsNullable = true)]
        public int? PasswordAge { get; set; }

        /// <summary>
        /// upper case complexity
        /// </summary>
        [DataMember(Name = "upperCaseComplexity")]
        [JsonProperty("upperCaseComplexity")]
        [XmlElement(ElementName = "upperCaseComplexity", IsNullable = true)]
        public KalturaUpperCaseComplexity UpperCaseComplexity { get; set; }

        internal override ICrudHandler<PasswordPolicy, long, PasswordPolicyFilter> Handler
        {
            get
            {
                return PasswordPolicyManager.Instance;
            }
        }

        internal override void SetId(long id)
        {
            throw new System.NotImplementedException();
        }

        internal override void ValidateForAdd()
        {
            throw new System.NotImplementedException();
        }

        internal override void ValidateForUpdate()
        {
            throw new System.NotImplementedException();
        }
    }

    public partial class KalturaPasswordPolicyListResponse : KalturaListResponse<KalturaPasswordPolicy>
    {
        public KalturaPasswordPolicyListResponse() : base() { }
    }
}