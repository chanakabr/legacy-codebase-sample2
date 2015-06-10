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
    /// LogIn
    /// </summary>
    public class LogIn
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

        /// <summary>
        /// keyValues
        /// </summary>        
        [DataMember(Name = "keyValues")]
        [JsonProperty("keyValues")]        
        public Dictionary<string, string> keyValues { get; set; }
    }
}