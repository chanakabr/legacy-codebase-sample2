using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.API;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// SimpleExpression hold single condition
    /// </summary>
    public partial class KalturaUserSessionCondition : KalturaUserSessionProfileExpression
    {
        /// <summary>
        /// expression
        /// </summary>
        [DataMember(Name = "condition")]
        [JsonProperty("condition")]
        [XmlElement(ElementName = "condition")]
        public KalturaCondition Condition { get; set; }
    }
}