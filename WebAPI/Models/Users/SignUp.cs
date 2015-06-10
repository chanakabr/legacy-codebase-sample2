using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// SignUp
    /// </summary>
    public class SignUp
    {
        /// <summary>
        /// User Basic Data
        /// </summary>
        [DataMember(Name = "user_basic_data")]
        [JsonProperty("user_basic_data")]
        [Required]
        public UserBasicData userBasicData { get; set; }

        /// <summary>
        /// User Dynamic Data
        /// </summary>
        [DataMember(Name = "user_dynamic_data")]
        [JsonProperty("user_dynamic_data")]
        [Required]
        public Dictionary<string,string> userDynamicData { get; set; }

         /// <summary>
        /// password
        /// </summary>
        [DataMember(Name = "password")]
        [JsonProperty("password")]
        public string password { get; set; }

        /// <summary>
        /// affiliateCode
        /// </summary>
        [DataMember(Name = "affiliateCode")]
        [JsonProperty("affiliateCode")]
        public string affiliateCode { get; set; }

    }
}