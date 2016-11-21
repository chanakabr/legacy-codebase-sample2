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
    public class KalturaAssetFileContext : KalturaOTTObject
    {
        /// <summary>
        /// viewLifeCycle
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        [SchemeProperty(ReadOnly = true)]
        public string ViewLifeCycle { get; set; }

        /// <summary>
        /// fullLifeCycle
        /// </summary>
        [DataMember(Name = "fullLifeCycle")]
        [JsonProperty("fullLifeCycle")]
        [XmlElement(ElementName = "fullLifeCycle")]
        [SchemeProperty(ReadOnly = true)]
        public string FullLifeCycle { get; set; }

        /// <summary>
        /// isOfflinePlayBack
        /// </summary>
        [DataMember(Name = "isOfflinePlayBack")]
        [JsonProperty("isOfflinePlayBack")]
        [XmlElement(ElementName = "isOfflinePlayBack")]
        [SchemeProperty(ReadOnly = true)]
        public bool IsOfflinePlayBack { get; set; }

        public enum KalturaContextType
        {
            none = 0,
            recording = 1
        }
    }
}