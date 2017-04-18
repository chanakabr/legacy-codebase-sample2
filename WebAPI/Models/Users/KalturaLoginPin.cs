using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Log in pin code details
    /// </summary>
    public class KalturaUserLoginPin : KalturaOTTObject
    {
        /// <summary>
        /// Generated login pin code
        /// </summary>
        [DataMember(Name = "pinCode")]
        [JsonProperty(PropertyName = "pinCode")]
        [XmlElement(ElementName = "pinCode")]
        [OldStandardProperty("pin_code")]
        public string PinCode { get; set; }

        /// <summary>
        /// Login pin expiration time (epoch)
        /// </summary>
        [DataMember(Name = "expirationTime")]
        [JsonProperty(PropertyName = "expirationTime")]
        [XmlElement(ElementName = "expirationTime")]
        [OldStandardProperty("expiration_time")]
        public long? ExpirationTime { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        [SchemeProperty(ReadOnly = true)]
        [OldStandardProperty("user_id")]
        public string UserId { get; set; }
    }
}