using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// SignUp
    /// </summary>
    public class KalturaSignUp : KalturaOTTObject
    {
        /// <summary>
        /// Basic Data
        /// </summary>
        [DataMember(Name = "basic_data")]
        [JsonProperty("basic_data")]
        [Required]
        public KalturaUserBasicData userBasicData { get; set; }

        /// <summary>
        /// User Dynamic Data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        [Required]
        public Dictionary<string, string> userDynamicData { get; set; }

        /// <summary>
        /// Desired Password
        /// </summary>
        [DataMember(Name = "password")]
        [JsonProperty("password")]
        public string password { get; set; }

        /// <summary>
        /// Affiliate code
        /// </summary>
        [DataMember(Name = "affiliat_code")]
        [JsonProperty("affiliat_code")]
        public string affiliateCode { get; set; }

    }
}