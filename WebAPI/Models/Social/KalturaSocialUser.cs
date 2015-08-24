using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using WebAPI.Models.General;
using System.Xml.Serialization;

namespace WebAPI.Models.Social
{
    public class KalturaSocialUser : KalturaOTTObject
    {
        /// <summary>
        /// Facebook identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string ID { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "first_name")]
        [JsonProperty("first_name")]
        [XmlElement(ElementName = "first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "last_name")]
        [JsonProperty("last_name")]
        [XmlElement(ElementName = "last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// User email
        /// </summary>
        [DataMember(Name = "email")]
        [JsonProperty("email")]
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [DataMember(Name = "gender")]
        [JsonProperty("gender")]
        [XmlElement(ElementName = "gender")]
        public string Gender { get; set; }

        /// <summary>
        /// User identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty("user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserId { get; set; }

        /// <summary>
        /// User birthday
        /// </summary>
        [DataMember(Name = "birthday")]
        [JsonProperty("birthday")]
        [XmlElement(ElementName = "birthday")]
        public string Birthday { get; set; }
    }
}