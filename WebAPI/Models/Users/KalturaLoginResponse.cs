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
    public class KalturaLoginResponse : KalturaOTTObject
    {
        /// <summary>
        /// User
        /// </summary>
        [DataMember(Name = "user")]
        [JsonProperty("user")]
        [XmlElement(ElementName = "user", IsNullable = true)]
        public KalturaOTTUser User { get; set; }

        /// <summary>
        /// Kaltura login session details
        /// </summary>
        [DataMember(Name = "loginSession")]
        [JsonProperty("loginSession")]
        [XmlElement(ElementName = "loginSession", IsNullable = true)]
        [OldStandardProperty("login_session")]
        public KalturaLoginSession LoginSession { get; set; }
    }
}