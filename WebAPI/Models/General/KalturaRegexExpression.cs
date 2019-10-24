using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// KalturaRegexExpression
    /// </summary>
    public partial class KalturaRegexExpression : KalturaOTTObject /*: KalturaPasswordPolicyComplexity*/
    {
        /// <summary>
        /// regex expression 
        /// </summary>
        [DataMember(Name = "expression")]
        [JsonProperty("expression")]
        [XmlElement(ElementName = "expression")]
        [SchemeInput(MinLength = 1)]
        public string Expression { get; set; }

        /// <summary>
        /// description 
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description")]
        [SchemeInput(MinLength = 1)]
        public string Description { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(this.Expression))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            if (string.IsNullOrEmpty(this.Description))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            try
            {
                Regex.Match("", this.Expression);
            }
            catch (System.ArgumentException)
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "expression");
            }
        }
    }
}