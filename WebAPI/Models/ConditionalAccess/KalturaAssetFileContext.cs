using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaAssetFileContext : KalturaOTTObject
    {
        /// <summary>
        /// viewLifeCycle
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        public string ViewLifeCycle { get; set; }

        /// <summary>
        /// fullLifeCycle
        /// </summary>
        [DataMember(Name = "fullLifeCycle")]
        [JsonProperty("fullLifeCycle")]
        [XmlElement(ElementName = "fullLifeCycle")]
        public string FullLifeCycle { get; set; }

        /// <summary>
        /// isOfflinePlayBack
        /// </summary>
        [DataMember(Name = "isOfflinePlayBack")]
        [JsonProperty("isOfflinePlayBack")]
        [XmlElement(ElementName = "isOfflinePlayBack")]
        public bool IsOfflinePlayBack { get; set; }
    }
}