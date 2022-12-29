using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// Not Expression
    /// </summary>
    public partial class KalturaExpressionNot : KalturaUserSessionProfileExpression
    {
        /// <summary>
        /// expression
        /// </summary>
        [DataMember(Name = "expression")]
        [JsonProperty("expression")]
        [XmlElement(ElementName = "expression")]
        public KalturaUserSessionProfileExpression Expression { get; set; }
    }
}