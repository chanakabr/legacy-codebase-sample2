using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;

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

        internal override int ConditionsSum()
        {
            return Expression.ConditionsSum();
        }
        
        internal override void Validate()
        {
            if (Expression == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            Expression.Validate();
        }
    }
}