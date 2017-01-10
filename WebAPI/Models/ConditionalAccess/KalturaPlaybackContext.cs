using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPlaybackContext : KalturaOTTObject
    {
        /// <summary>
        /// Sources
        /// </summary>
        [DataMember(Name = "sources")]
        [JsonProperty("sources")]
        [XmlElement(ElementName = "sources")]
        public List<KalturaPlaybackSource> Sources { get; set; }

        /// <summary>
        /// Actions
        /// </summary>
        [DataMember(Name = "actions")]
        [JsonProperty("actions")]
        [XmlElement(ElementName = "actions")]
        public List<KalturaRuleAction> Actions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "messages")]
        [JsonProperty("messages")]
        [XmlElement(ElementName = "messages")]
        public List<KalturaAccessControlMessage> Messages { get; set; }
    }
}