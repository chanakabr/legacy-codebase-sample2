using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Log in pin code details
    /// </summary>
    public class KalturaLoginPin : KalturaOTTObject
    {
        /// <summary>
        /// Generated login pin code
        /// </summary>
        [DataMember(Name = "pin_code")]
        [JsonProperty(PropertyName = "pin_code")]
        [XmlElement(ElementName = "pin_code")]
        public string PinCode { get; set; }

        /// <summary>
        /// Login pin expiration time (epoch)
        /// </summary>
        [DataMember(Name = "expiration_time")]
        [JsonProperty(PropertyName = "expiration_time")]
        [XmlElement(ElementName = "expiration_time")]
        public long? ExpirationTime { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        [DataMember(Name = "user_id")]
        [JsonProperty(PropertyName = "user_id")]
        [XmlElement(ElementName = "user_id")]
        public string UserId { get; set; }
    }
}