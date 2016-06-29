using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Log in pin code details
    /// </summary>
    [OldStandard("pinCode", "pin_code")]
    [OldStandard("expirationTime", "expiration_time")]
    [OldStandard("userId", "user_id")]
    public class KalturaLoginPin : KalturaOTTObject
    {
        /// <summary>
        /// Generated login pin code
        /// </summary>
        [DataMember(Name = "pinCode")]
        [JsonProperty(PropertyName = "pinCode")]
        [XmlElement(ElementName = "pinCode")]
        public string PinCode { get; set; }

        /// <summary>
        /// Login pin expiration time (epoch)
        /// </summary>
        [DataMember(Name = "expirationTime")]
        [JsonProperty(PropertyName = "expirationTime")]
        [XmlElement(ElementName = "expirationTime")]
        public long? ExpirationTime { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId { get; set; }
    }
}