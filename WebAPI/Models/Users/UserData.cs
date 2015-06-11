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
    /// User Data
    /// </summary>
    public class UserData
    {

        /// <summary>
        /// site guid
        /// </summary>
        [DataMember(Name = "siteguid")]
        [JsonProperty("siteguid")]
        [Required]
        public string siteGuid { get; set; }
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
        public Dictionary<string, string> userDynamicData { get; set; }

    }
}