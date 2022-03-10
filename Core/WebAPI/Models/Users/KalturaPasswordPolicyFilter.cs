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
    public partial class KalturaPasswordPolicyFilter : KalturaFilter<KalturaPasswordPolicyOrderBy>
    {
        /// <summary>
        /// Comma separated list of role Ids.
        /// </summary>
        [DataMember(Name = "userRoleIdIn")]
        [JsonProperty("userRoleIdIn")]
        [XmlElement(ElementName = "userRoleIdIn")]
        [SchemeProperty(DynamicMinInt = 0)]
        public string UserRoleIdIn { get; set; }

        public override KalturaPasswordPolicyOrderBy GetDefaultOrderByValue()
        {
            return KalturaPasswordPolicyOrderBy.NONE;
        }
    }
}