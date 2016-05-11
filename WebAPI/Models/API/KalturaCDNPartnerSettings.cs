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
        [DataMember(Name = "defaultVodAdapter")]
        [JsonProperty("defaultVodAdapter")]
        [XmlElement(ElementName = "defaultVodAdapter", IsNullable = true)]
        public int? DefaultVodAdapter { get; set; }

        /// <summary>
        /// Default live and catch-up CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultEpgAdapter")]
        [JsonProperty("defaultEpgAdapter")]
        [XmlElement(ElementName = "defaultEpgAdapter", IsNullable = true)]
        public int? DefaultEpgAdapter { get; set; }

        /// <summary>
        /// Default recording CDN adapter identifier
        /// </summary>
        [DataMember(Name = "defaultRecordingAdapter")]
        [JsonProperty("defaultRecordingAdapter")]
        [XmlElement(ElementName = "defaultRecordingAdapter", IsNullable = true)]
        public int? DefaultRecordingAdapter { get; set; }

    }
}