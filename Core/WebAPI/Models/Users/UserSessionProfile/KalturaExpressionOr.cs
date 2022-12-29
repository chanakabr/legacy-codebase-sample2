using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// Or Expression
    /// </summary>
    public partial class KalturaExpressionOr : KalturaUserSessionProfileExpression
    {
        /// <summary>
        /// expressions with or relation between them
        /// </summary>
        [DataMember(Name = "expressions")]
        [JsonProperty("expressions")]
        [XmlElement(ElementName = "expressions")]
        [SchemeProperty(MinItems = 2)]
        public List<KalturaUserSessionProfileExpression> Expressions { get; set; }
    }
}