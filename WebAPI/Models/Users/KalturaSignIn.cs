using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// SignIn
    /// </summary>
    public class KalturaSignIn : KalturaOTTObject
    {
        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        [XmlElement(ElementName = "username")]
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>        
        [DataMember(Name = "password")]
        [JsonProperty("password")]
        [XmlElement(ElementName = "password")]
        [Required]
        public string Password { get; set; }
      
    }
}