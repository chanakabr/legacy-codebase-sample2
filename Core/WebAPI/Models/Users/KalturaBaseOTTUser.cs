using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Slim user data
    /// </summary>
    [XmlInclude(typeof(KalturaOTTUser))]
    public partial class KalturaBaseOTTUser : KalturaOTTObject
    {
        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        [XmlElement(ElementName = "username")]
        [SchemeProperty(MaxLength = 256)]
        public string Username { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "firstName")]
        [JsonProperty("firstName")]
        [XmlElement(ElementName = "firstName")]
        [OldStandardProperty("first_name")]
        [SchemeProperty(MaxLength = 128)]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "lastName")]
        [JsonProperty("lastName")]
        [XmlElement(ElementName = "lastName")]
        [OldStandardProperty("last_name")]
        [SchemeProperty(MaxLength = 128)]
        public string LastName { get; set; }
    }
}