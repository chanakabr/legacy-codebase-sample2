using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// And Expression
    /// </summary>
    [Serializable]
    public partial class KalturaExpressionAnd : KalturaUserSessionProfileExpression
    {
        /// <summary>
        /// expressions with and relation between them
        /// </summary>
        [DataMember(Name = "expressions")]
        [JsonProperty("expressions")]
        [XmlElement(ElementName = "expressions")]
        [SchemeProperty(MinItems = 1)]
        public List<KalturaUserSessionProfileExpression> Expressions { get; set; }
    }
}