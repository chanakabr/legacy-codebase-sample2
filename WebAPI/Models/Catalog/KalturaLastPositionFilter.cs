using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering last position requests
    /// </summary>
    [Serializable]
    public class KalturaLastPositionFilter : KalturaOTTObject
    {
        /// <summary>
        /// Device UDID
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]        
        public string UDID { get; set; }

        /// <summary>
        /// media identifier
        /// </summary>
        [DataMember(Name = "media_id")]
        [JsonProperty(PropertyName = "media_id")]
        [XmlElement(ElementName = "media_id")]
        public int? MediaID { get; set; }

        /// <summary>
        /// nPVR identifier
        /// </summary>
        [DataMember(Name = "npvr_id")]
        [JsonProperty(PropertyName = "npvr_id")]
        [XmlElement(ElementName = "npvr_id")]
        public string NPVRID { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}