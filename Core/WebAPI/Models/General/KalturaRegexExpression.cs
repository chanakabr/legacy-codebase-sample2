using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
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
    }
}