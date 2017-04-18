using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Slim user data
    /// </summary>
    [XmlInclude(typeof(KalturaOTTUser))]
    public class KalturaBaseOTTUser : KalturaOTTObject
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
        public string Username { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "firstName")]
        [JsonProperty("firstName")]
        [XmlElement(ElementName = "firstName")]
        [OldStandardProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "lastName")]
        [JsonProperty("lastName")]
        [XmlElement(ElementName = "lastName")]
        [OldStandardProperty("last_name")]
        public string LastName { get; set; }
    }
}