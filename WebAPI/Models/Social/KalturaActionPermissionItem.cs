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
        /// SocialNetwork 
        /// </summary
        [DataMember(Name = "network")]
        [JsonProperty("network")]
        [XmlElement(ElementName = "network", IsNullable = true)]
        [SchemeProperty()]
        public KalturaSocialNetwork? Network { get; set; }

        /// <summary>
        /// SocialActionPrivacy 
        /// </summary>
        [DataMember(Name = "socialActionPrivacy")]
        [JsonProperty("socialActionPrivacy")]
        [XmlElement(ElementName = "socialActionPrivacy")]
        [SchemeProperty()]
        public KalturaSocialActionPrivacy SocialActionPrivacy { get; set; }

        /// <summary>
        /// SocialPrivacy 
        /// </summary>
        [DataMember(Name = "socialPrivacy")]
        [JsonProperty("socialPrivacy")]
        [XmlElement(ElementName = "socialPrivacy")]
        [SchemeProperty()]
        public KalturaSocialPrivacy SocialPrivacy { get; set; }
    }
}