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

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings
    /// </summary>
    [Serializable]
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
        public List<KalturaRegex> Complexities { get; set; }

        /// <summary>
        ///  the number of passwords failures before the account is locked.
        /// </summary>
        [DataMember(Name = "lockoutFailuresCount")]
        [JsonProperty("lockoutFailuresCount")]
        [XmlElement(ElementName = "lockoutFailuresCount", IsNullable = true)]
        public int? LockoutFailuresCount { get; set; }

        public KalturaPasswordPolicy() : base() { }

        /*
        /// <summary>
        /// lower case complexity
        /// </summary>
        [DataMember(Name = "lowerCaseComplexity")]
        [JsonProperty("lowerCaseComplexity")]
        [XmlElement(ElementName = "lowerCaseComplexity", IsNullable = true)]
        public KalturaLowerCaseComplexity LowerCaseComplexity { get; set; }

        /// <summary>
        /// numbers case complexity
        /// </summary>
        [DataMember(Name = "numbersComplexity")]
        [JsonProperty("numbersComplexity")]
        [XmlElement(ElementName = "numbersComplexity", IsNullable = true)]
        public KalturaNumbersComplexity NumbersComplexity { get; set; }

        /// <summary>
        /// special Characters Complexity 
        /// </summary>
        [DataMember(Name = "specialCharactersComplexity")]
        [JsonProperty("specialCharactersComplexity")]
        [XmlElement(ElementName = "specialCharactersComplexity", IsNullable = true)]
        public KalturaSpecialCharactersComplexity SpecialCharactersComplexity { get; set; }

        /// <summary>
        /// special Characters Complexity 
        /// </summary>
        [DataMember(Name = "identicalCharactersComplexity")]
        [JsonProperty("identicalCharactersComplexity")]
        [XmlElement(ElementName = "identicalCharactersComplexity", IsNullable = true)]
        public KalturaIdenticalCharactersComplexity IdenticalCharactersComplexity { get; set; }

        /// <summary>
        /// special Characters Complexity 
        /// </summary>
        [DataMember(Name = "passwordHistory")]
        [JsonProperty("passwordHistory")]
        [XmlElement(ElementName = "passwordHistory", IsNullable = true)]
        public int? PasswordHistory { get; set; }
        */

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
            if (!this.ValidateRegexExpressions())
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "code");
            }
        }

        internal override void ValidateForUpdate()
        {
            if (!this.ValidateRegexExpressions())
            {
                throw new System.NotImplementedException();
            }
        }

        internal bool ValidateRegexExpressions()
        {
            if (this.Complexities == null || this.Complexities.Count == 0)
            {
                return true;
            }

            foreach (var pattern in this.Complexities)
            {
                if (string.IsNullOrEmpty(pattern.Expression)) return false;

                try
                {
                    System.Text.RegularExpressions.Regex.Match("", pattern.Expression);
                }
                catch (System.ArgumentException)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public partial class KalturaPasswordPolicyListResponse : KalturaListResponse<KalturaPasswordPolicy>
    {
        public KalturaPasswordPolicyListResponse() : base() { }
    }
}