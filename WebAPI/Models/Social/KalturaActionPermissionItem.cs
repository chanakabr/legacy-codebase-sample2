using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaActionPermissionItem : KalturaOTTObject
    {
        /// <summary>
        /// Social network 
        /// </summary>
        [DataMember(Name = "network")]
        [JsonProperty("network")]
        [XmlElement(ElementName = "network", IsNullable = true)]
        [SchemeProperty()]
        public KalturaSocialNetwork? Network { get; set; }

        /// <summary>
        /// Action privacy 
        /// </summary>
        [DataMember(Name = "actionPrivacy")]
        [JsonProperty("actionPrivacy")]
        [XmlElement(ElementName = "actionPrivacy")]
        [SchemeProperty()]
        public KalturaSocialActionPrivacy ActionPrivacy { get; set; }

        /// <summary>
        /// Social privacy
        /// </summary>
        [DataMember(Name = "privacy")]
        [JsonProperty("privacy")]
        [XmlElement(ElementName = "privacy")]
        [SchemeProperty()]
        public KalturaSocialPrivacy Privacy { get; set; }
    }
}