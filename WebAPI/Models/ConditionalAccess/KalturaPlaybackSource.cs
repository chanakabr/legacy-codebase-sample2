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
    public class KalturaPlaybackSource : KalturaMediaFile
    {
        /// <summary>
        /// Source format according to delivery profile streamer type (applehttp, mpegdash etc.)
        /// </summary>
        [DataMember(Name = "format")]
        [JsonProperty("format")]
        [XmlElement(ElementName = "format")]
        public string Format { get; set; }

        /// <summary>
        /// Comma separated string according to deliveryProfile media protocols ('http,https' etc.)
        /// </summary>
        [DataMember(Name = "protocols")]
        [JsonProperty("protocols")]
        [XmlElement(ElementName = "protocols")]
        public string Protocols { get; set; }

        /// <summary>
        /// DRM data object containing relevant license URL ,scheme name and certificate
        /// </summary>
        [DataMember(Name = "drm")]
        [JsonProperty("drm")]
        [XmlElement(ElementName = "drm")]
        public List<KalturaDrmPlaybackPluginData> Drm { get; set; }

        internal int DrmId;
    }
}