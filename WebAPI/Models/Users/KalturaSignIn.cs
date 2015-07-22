using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// SignIn
    /// </summary>
    public class KalturaSignIn
    {
        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>        
        [DataMember(Name = "password")]        
        [JsonProperty("password")]
        [Required]
        public string Password { get; set; }
      
    }
}