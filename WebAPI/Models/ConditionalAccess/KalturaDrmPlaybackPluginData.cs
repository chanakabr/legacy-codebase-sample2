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
    public class KalturaDrmPlaybackPluginData : KalturaPluginData
    {
        /// <summary>
        /// Scheme
        /// </summary>
        [DataMember(Name = "scheme")]
        [JsonProperty("scheme")]
        [XmlElement(ElementName = "scheme")]
        public KalturaDrmSchemeName Scheme { get; set; }

        /// <summary>
        /// License URL
        /// </summary>
        [DataMember(Name = "licenseURL")]
        [JsonProperty("licenseURL")]
        [XmlElement(ElementName = "licenseURL")]
        public string LicenseURL { get; set; }

        /// <summary>
        /// Custom data string
        /// </summary>
        [DataMember(Name = "customDataString")]
        [JsonProperty("customDataString")]
        [XmlElement(ElementName = "customDataString")]
        public string CustomDataString { get; set; }

        /// <summary>
        /// Signature string
        /// </summary>
        [DataMember(Name = "signature")]
        [JsonProperty("signature")]
        [XmlElement(ElementName = "signature")]
        public string Signature { get; set; }
    }

    public class KalturaPluginData : KalturaOTTObject
    {
    }

    public class KalturaFairPlayPlaybackPluginData : KalturaDrmPlaybackPluginData
    {
        /// <summary>
        /// Custom data string
        /// </summary>
        [DataMember(Name = "certificate")]
        [JsonProperty("certificate")]
        [XmlElement(ElementName = "certificate")]
        public string Certificate { get; set; }
    }

    public enum KalturaDrmSchemeName
    {
        PLAYREADY_CENC,
        WIDEVINE_CENC,
        FAIRPLAY,
        WIDEVINE,
        PLAYREADY
    }
}