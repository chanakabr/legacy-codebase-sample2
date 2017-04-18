using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaLicensedUrl : KalturaOTTObject
    {
        /// <summary>
        /// Main licensed URL
        /// </summary>
        [DataMember(Name = "mainUrl")]
        [JsonProperty("mainUrl")]
        [XmlElement(ElementName = "mainUrl")]
        [OldStandardProperty("main_url")]
        public string MainUrl { get; set; }

        /// <summary>
        /// An alternate URL to use in case the main fails
        /// </summary>
        [DataMember(Name = "altUrl")]
        [JsonProperty("altUrl")]
        [XmlElement(ElementName = "altUrl")]
        [OldStandardProperty("alt_url")]
        public string AltUrl { get; set; }
    }
}