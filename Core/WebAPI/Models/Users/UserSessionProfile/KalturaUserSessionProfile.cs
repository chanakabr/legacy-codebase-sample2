using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// User Session Profile
    /// </summary>
    public partial class KalturaUserSessionProfile : KalturaOTTObject
    {
        /// <summary>
        ///  The user session profile id.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// The user session profile name for presentation.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(Pattern = SchemeInputAttribute.ASCII_ONLY_PATTERN)]
        public string Name { get; set; }

        /// <summary>
        /// expression
        /// </summary>
        [DataMember(Name = "expression")]
        [JsonProperty("expression")]
        [XmlElement(ElementName = "expression")]
        public KalturaUserSessionProfileExpression Expression { get; set; }
    }
}