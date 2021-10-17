using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;

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

        internal override int ConditionsSum()
        {
            return this.Expressions.Sum(_ => _.ConditionsSum());
        }

        internal override void Validate()
        {
            foreach (var item in Expressions)
            {
                item.Validate();
            }
        }
    }
}