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
    public partial class KalturaCaptionPlaybackPluginData
    {
        /// <summary>
        /// url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        public string URL { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty("language")]
        [XmlElement(ElementName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Label
        /// </summary>
        [DataMember(Name = "label")]
        [JsonProperty("label")]
        [XmlElement(ElementName = "label")]
        public string Label { get; set; }       

        /// <summary>
        /// Format
        /// </summary>
        [DataMember(Name = "format")]
        [JsonProperty("format")]
        [XmlElement(ElementName = "format")]
        public string Format { get; set; }
    }
}