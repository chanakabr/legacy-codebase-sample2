using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaCDNPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Default CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultAdapterId")]
        [JsonProperty("defaultAdapterId")]
        [XmlElement(ElementName = "defaultAdapterId", IsNullable = true)]
        public int? DefaultAdapterId { get; set; }

        /// <summary>
        /// Default recording CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultRecordingAdapterId")]
        [JsonProperty("defaultRecordingAdapterId")]
        [XmlElement(ElementName = "defaultRecordingAdapterId", IsNullable = true)]
        public int? DefaultRecordingAdapterId { get; set; }

    }
}