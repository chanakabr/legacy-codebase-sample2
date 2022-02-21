using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy settings
    /// </summary>
    [Serializable]
    public partial class KalturaPasswordPolicy : KalturaOTTObjectSupportNullable
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
    }
}