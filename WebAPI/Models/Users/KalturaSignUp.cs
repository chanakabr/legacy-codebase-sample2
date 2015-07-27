using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models.General;
using System.Xml.Serialization;

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
        [XmlElement(ElementName = "basic_data")]
        [Required]
        public KalturaUserBasicData userBasicData { get; set; }

        /// <summary>
        /// User Dynamic Data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        [XmlElement(ElementName = "dynamic_data")]
        [Required]
        public Dictionary<string, string> userDynamicData { get; set; }

        /// <summary>
        /// Desired Password
        /// </summary>
        [DataMember(Name = "password")]
        [JsonProperty("password")]
        [XmlElement(ElementName = "password")]
        public string password { get; set; }

        /// <summary>
        /// Affiliate code
        /// </summary>
        [DataMember(Name = "affiliate_code")]
        [JsonProperty("affiliate_code")]
        [XmlElement(ElementName = "affiliate_code")]
        public string affiliateCode { get; set; }

    }
}