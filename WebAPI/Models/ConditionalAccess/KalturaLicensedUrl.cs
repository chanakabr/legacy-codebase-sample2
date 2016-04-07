using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrl : KalturaOTTObject
    {
        /// <summary>
        /// Main licensed URL
        /// </summary>
        [DataMember(Name = "main_url")]
        [JsonProperty("main_url")]
        [XmlElement(ElementName = "main_url")]
        public string MainUrl { get; set; }

        /// <summary>
        /// An alternate URL to use in case the main fails
        /// </summary>
        [DataMember(Name = "alt_url")]
        [JsonProperty("alt_url")]
        [XmlElement(ElementName = "alt_url")]
        public string AltUrl { get; set; }
    }
}