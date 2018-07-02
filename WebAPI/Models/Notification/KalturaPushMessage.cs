using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "KalturaPushMessage", Namespace = "")]
    [XmlRoot("KalturaPushMessage")]
    public partial class KalturaPushMessage : KalturaOTTObject
    {
        /// <summary>
        /// The message that will be presented to the user.
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        [SchemeProperty(MinLength = 1)]
        public string Message { get; set; }

        /// <summary>
        /// Optional. Can be used to change the default push sound on the user device.
        /// </summary>
        [DataMember(Name = "sound")]
        [JsonProperty("sound")]
        [XmlElement(ElementName = "sound")]
        public string Sound { get; set; }

        /// <summary>
        /// Optional. Used to change the default action of the application when a push is received.
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        /// <summary>
        /// Optional. Used to direct the application to the relevant page.
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string Url { get; set; }
    }
}