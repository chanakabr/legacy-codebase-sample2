using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Container for translation
    /// </summary>
    public partial class KalturaTranslationToken : KalturaOTTObject
    {
        /// <summary>
        /// Language code
        /// </summary>
        [DataMember(Name = "language")]
        [JsonProperty("language")]
        [XmlElement(ElementName = "language")]
        public string Language { get; set; }

        /// <summary>
        /// Translated value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty("value")]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }
}