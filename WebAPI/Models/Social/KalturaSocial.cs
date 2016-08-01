using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using WebAPI.Models.General;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Social
{
    abstract public class KalturaSocial : KalturaOTTObject
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
        [DataMember(Name = "firstName")]
        [JsonProperty("firstName")]
        [XmlElement(ElementName = "firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "lastName")]
        [JsonProperty("lastName")]
        [XmlElement(ElementName = "lastName")]
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
        [DataMember(Name = "userId")]
        [JsonProperty("userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }

        /// <summary>
        /// User birthday
        /// </summary>
        [DataMember(Name = "birthday")]
        [JsonProperty("birthday")]
        [XmlElement(ElementName = "birthday")]
        public string Birthday { get; set; }

        /// <summary>
        /// User model status
        /// Possible values: UNKNOWN, OK, ERROR, NOACTION, NOTEXIST, CONFLICT, MERGE, MERGEOK, NEWUSER, MINFRIENDS, INVITEOK, INVITEERROR, ACCESSDENIED, WRONGPASSWORDORUSERNAME, UNMERGEOK
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Profile picture URL
        /// </summary>
        [DataMember(Name = "pictureUrl")]
        [JsonProperty("pictureUrl")]
        [XmlElement(ElementName = "pictureUrl")]
        public string PictureUrl { get; set; }
    }
}