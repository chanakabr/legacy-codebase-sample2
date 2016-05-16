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
        /// Default VOD CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultVodAdapterId")]
        [JsonProperty("defaultVodAdapterId")]
        [XmlElement(ElementName = "defaultVodAdapterId", IsNullable = true)]
        public int? DefaultVodAdapterId { get; set; }

        /// <summary>
        /// Default live and catch-up CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultEpgAdapterId")]
        [JsonProperty("defaultEpgAdapterId")]
        [XmlElement(ElementName = "defaultEpgAdapterId", IsNullable = true)]
        public int? DefaultEpgAdapterId { get; set; }

        /// <summary>
        /// Default recording CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultRecordingAdapterId")]
        [JsonProperty("defaultRecordingAdapterId")]
        [XmlElement(ElementName = "defaultRecordingAdapterId", IsNullable = true)]
        public int? DefaultRecordingAdapterId { get; set; }

    }
}