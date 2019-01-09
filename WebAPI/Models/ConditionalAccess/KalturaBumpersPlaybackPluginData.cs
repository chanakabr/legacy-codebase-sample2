using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaBumpersPlaybackPluginData : KalturaOTTObject
    {
        /// <summary>
        /// url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string URL { get; set; }

        /// <summary>
        /// Streamer type: hls, dash, progressive.
        /// </summary>
        [DataMember(Name = "streamertype")]
        [JsonProperty("streamertype")]
        [XmlElement(ElementName = "streamertype")]
        public string StreamerType { get; set; }
    }
}